import Link from 'next/link';
import { Lockup } from '@/components/lockup';
import { SignedInRedirect } from '@/components/signed-in-redirect';
import styles from './page.module.css';

/**
 * The landing page.
 *
 * A SERVER component — it renders to HTML at build time and ships no application bundle. That is ADR-0009's
 * requirement, and it survives ADR-0022 unchanged: a reader who came to read must not download an
 * application, and a crawler must see prose rather than a spinner.
 *
 * "Sign in" now goes to a route in THIS application, on THIS origin. Under the old split it handed the
 * reader to a different app on a different port — which is not a product, it is two halves of one wearing the
 * same colours.
 */
const PILLARS = [
  {
    title: 'Why, then how',
    body: 'A topic opens with code you would actually write, shows it fail, and derives the concept from the failure. The name comes last — because that is when it has somewhere to land.',
  },
  {
    title: 'One concept, many ecosystems',
    body: 'Connection pooling exists for a reason that has nothing to do with C#. The reasoning is written once; the implementation panel switches between .NET, Java and Node. The reason transfers — that is the point.',
  },
  {
    title: 'Nothing publishes unreviewed',
    body: 'A model can draft a topic. A model cannot publish one. Every technical term must survive translation verbatim, and the save refuses content that fails the rule.',
  },
];

export default function LandingPage() {
  return (
    <main className={styles.page}>
      <SignedInRedirect />

      <header className={styles.hero}>
        {/*
          420, because the mark it replaced rendered ~448 wide here and the hero was built around that. The
          source is 600 across, so this is a downscale — crisp on a 2x display, which a hero has to be.
        */}
        <Lockup width={420} priority className={styles.heroLockup} />

        <h1>Why before how.</h1>

        <p className={styles.lede}>
          Most documentation tells you <em>how</em>. WhyStack tells you <strong>why</strong> — why a
          technology exists, which problem it actually solves, what it costs you, and when not to reach for
          it.
        </p>

        <nav className={styles.actions} aria-label="Get started">
          <Link className={`${styles.button} ${styles.primary}`} href="/register">
            Create an account
          </Link>

          <Link className={styles.button} href="/sign-in">
            Sign in
          </Link>
        </nav>
      </header>

      <section className={styles.pillars} aria-label="What makes this different">
        {PILLARS.map((pillar) => (
          <article key={pillar.title}>
            <h2>{pillar.title}</h2>
            <p>{pillar.body}</p>
          </article>
        ))}
      </section>

      <section className={styles.sample} aria-label="What a topic looks like">
        <h2>What a topic looks like</h2>

        <p>It opens with a bug you can write — in a language you may never use:</p>

        <pre>
          <code>{`char* name = malloc(64);
strcpy(name, "Ada");

free(name);                 // done with it

printf("%s\\n", name);       // ...one line too late`}</code>
        </pre>

        <p>
          Most days it prints <code>Ada</code>. Some days it prints whatever the allocator handed to the next
          caller — which, in a web server, is somebody else&rsquo;s session token. And it does not crash. That
          is the part worth sitting with.
        </p>

        <p className={styles.turn}>
          <strong>If you never free memory, who does?</strong>
        </p>

        <p>
          That question is what earns the <code>Common Language Runtime</code> a name, three paragraphs later.
          Not before.
        </p>
      </section>
    </main>
  );
}
