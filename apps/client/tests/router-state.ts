/**
 * Where the test says the learner currently is. Mutable, so a test can put them on /settings without
 * standing up a real router.
 *
 * It lives in its own module because vi.mock is hoisted to the top of the file that contains it, and
 * the mock has to be registered before any component imports expo-router for real. That means the
 * mock belongs in setup.ts — and setup.ts cannot both define the state and be imported by the tests
 * that need to change it.
 */
export const routerState: {
  pathname: string;
  /** What useSegments() returns. `['(auth)', 'sign-in']` puts the test on the sign-in screen. */
  segments: string[];
  /** The query string of the route — how a token arrives from an emailed link. */
  params: Record<string, string | undefined>;
  /** Every <Redirect> the tree rendered. This is how a gate test asserts where somebody was sent. */
  redirects: string[];
} = {
  pathname: '/',
  segments: [],
  params: {},
  redirects: [],
};
