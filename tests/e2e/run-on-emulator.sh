#!/usr/bin/env bash
#
# What runs inside the Android emulator job.
#
# It lives in a FILE rather than inline in the workflow, and that is not tidiness. The emulator action
# takes its `script` input and hands it to `sh -c "…"`, and a multi-line script with a `for` loop does
# not survive that intact:
#
#   /usr/bin/sh: 1: Syntax error: end of file unexpected (expecting "done")
#
# The emulator had booted. Maestro never ran. The job then sat there until the 45-minute timeout killed
# it, and reported "cancelled" — which looks like an infrastructure problem and is not one.
#
# A file has no quoting to survive. It is also runnable by a human, on their own machine, which an
# inline YAML string never is.
set -euo pipefail

API_URL="http://127.0.0.1:5207"
APK="apps/client/android/app/build/outputs/apk/release/app-release.apk"

echo "── Starting the stub API ──────────────────────────────────────────"

# TO A FILE, NOT THROUGH A PIPE — and killed on the way out. Both halves matter, and the second one is
# what cost an hour.
#
# It used to be `node … | tee stub-api.log &`. The flow passed in a hundred seconds; the job then sat
# there for fifty-eight minutes and was killed by the timeout, reported as "cancelled". It looked exactly
# like a hang in the test, and it was not: `tee` and `node` were still holding the script's stdout, the
# stub API never exits by design, so the pipe never closed — and the emulator action, which waits for the
# script's process tree, never saw it finish.
#
# The iOS job does the same thing and does NOT hang, which is why this was invisible for so long: there
# the stub runs in its own workflow step, and GitHub reaps orphaned processes at the end of the JOB
# ("Terminate orphan process: pid (37828) (node)" is in that log). Nothing reaps them here.
#
# So: no `tee`, and a trap that kills the stub and WAITS for it. Everything the console lost is printed
# from the file on the way out — including on failure, which is when it matters.
node tests/e2e/stub-api/server.mjs > stub-api.log 2>&1 &
STUB_PID=$!

cleanup() {
  status=$?

  echo "── The stub API's account of it ───────────────────────────────────"
  cat stub-api.log 2>/dev/null || true

  kill "${STUB_PID}" 2>/dev/null || true
  wait "${STUB_PID}" 2>/dev/null || true

  exit "${status}"
}

trap cleanup EXIT

for _ in $(seq 1 30); do
  if curl -fsS "${API_URL}/health" >/dev/null 2>&1; then
    break
  fi
  sleep 1
done

# Not `|| true`. If the stub is not up, every request the app makes will fail, the flow will land in the
# offline state, and the failure will look like an app bug. Fail here, where the cause is obvious.
curl -fsS "${API_URL}/health"
echo

echo "── Silencing Android's own crash dialogs ──────────────────────────"

# `hide_error_dialogs` suppresses the system's "X isn't responding" modals.
#
# This is not sweeping our failures under a rug — it is the opposite. A CI emulator runs on a software
# GPU on a shared machine, and ANDROID'S OWN processes miss their deadlines. The run before this one
# died on "Pixel Launcher isn't responding" — a modal from the HOME SCREEN, drawn on top of a WhyStack
# sign-in screen that had rendered perfectly. Maestro could not see past it, the first assertion failed,
# and the screenshot made it look as though our app had hung.
#
# It hides the DIALOG, not the failure. A real crash in our app still kills it, and every assertion
# after that still fails — loudly, and for the right reason.
adb shell settings put global hide_error_dialogs 1
adb shell am broadcast -a android.intent.action.CLOSE_SYSTEM_DIALOGS >/dev/null

echo "── Installing the app ─────────────────────────────────────────────"
test -f "${APK}" || { echo "No APK at ${APK}"; exit 1; }
adb install -r "${APK}"

echo "── Running the flow ───────────────────────────────────────────────"
maestro test tests/e2e/flows --format junit --output maestro-android.xml
