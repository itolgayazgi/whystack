import { useLocalSearchParams } from 'expo-router';
import { ConfirmEmailScreen } from '../../screens/auth/confirm-email-screen';

export default function ConfirmEmailRoute() {
  const { token } = useLocalSearchParams<{ token?: string }>();

  return <ConfirmEmailScreen token={token} />;
}
