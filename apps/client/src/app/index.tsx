import { HomeScreen } from '../screens/home-screen';

// Routes stay thin on purpose: a route decides *which* screen renders, the screen decides *what* it
// renders. Keeping screen bodies out of src/app means the route tree stays readable as a URL map.
export default function Index() {
  return <HomeScreen />;
}
