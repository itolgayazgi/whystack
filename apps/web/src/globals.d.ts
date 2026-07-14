// Next declares these itself, but only once its own type generation has run — and a fresh clone typechecks
// before it ever builds. Declaring them here means `pnpm typecheck` works on a machine that has never run
// `next build`, which is every CI machine.
declare module '*.css';

declare module '*.module.css' {
  const classes: Record<string, string>;
  export default classes;
}
