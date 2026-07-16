import type { ReactNode } from 'react';
import { Lockup } from '@/components/lockup';
import styles from './auth.module.css';

/**
 * The frame every authentication page sits in — and it is a DESKTOP frame, not a phone form on a big screen.
 *
 * `09`: <i>"Web should not simply stretch the mobile UI."</i> A 400-pixel column floating in the middle of a
 * 2560-pixel monitor is exactly that stretch, and it reads as an app somebody forgot to design for the screen
 * it is on.
 *
 * Two halves: the promise on the left, the form on the right. The promise is not decoration — it is the reason
 * somebody is filling in this form, and a sign-up page that does not say what it is for is a sign-up page
 * people abandon. Below 900px the left half disappears entirely; a phone browser gets the form and nothing
 * competing with it.
 */
export default function AuthLayout({ children }: { children: ReactNode }) {
  return (
    <main className={styles.split}>
      <aside className={styles.promise} aria-hidden="false">
        <Lockup width={360} priority className={styles.promiseLockup} />

        <h1>Why before how.</h1>

        <p>
          Learn technologies by the reasons they exist — which problem they actually solve, what they cost
          you, and when not to reach for them.
        </p>
      </aside>

      <section className={styles.panel}>{children}</section>
    </main>
  );
}
