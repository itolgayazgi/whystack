import { useLocalSearchParams } from 'expo-router';
import { ResetPasswordScreen } from '../../screens/auth/reset-password-screen';

// The token arrives in the query string of the link in the email — the API builds it (AppLinks) and a
// human clicks it. Only the ROUTE knows about the URL; the screen takes a token and knows nothing
// about how it got here, which is what lets it be tested without a router.
export default function ResetPasswordRoute() {
  const { token } = useLocalSearchParams<{ token?: string }>();

  return <ResetPasswordScreen token={token} />;
}
