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

# Output goes to a file AND to the console. The file is uploaded as an artifact: on a platform nobody
# here can run locally, the server's own account of what the app asked it is most of the debugging
# information there is.
node tests/e2e/stub-api/server.mjs 2>&1 | tee stub-api.log &

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
