import { Slot } from 'expo-router';

// The auth group has no chrome of its own: no tab bar, no sidebar. A sign-in screen that shows the
// product's navigation is a sign-in screen offering to take you somewhere you cannot go.
export default function AuthLayout() {
  return <Slot />;
}
