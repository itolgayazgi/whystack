/**
 * Where the test says the learner currently is. Mutable, so a test can put them on /settings without
 * standing up a real router.
 *
 * It lives in its own module because vi.mock is hoisted to the top of the file that contains it, and
 * the mock has to be registered before any component imports expo-router for real. That means the
 * mock belongs in setup.ts — and setup.ts cannot both define the state and be imported by the tests
 * that need to change it.
 */
export const routerState = { pathname: '/' };
