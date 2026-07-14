import { useLocalSearchParams } from 'expo-router';
import { TopicScreen } from '../../screens/topic-screen';

// `/topics/what-is-csharp`. The slug is the URL, and the same URL means the same topic for everyone who
// opens it — which is what makes a topic link shareable, cacheable and indexable (ADR-0009).

export default function TopicRoute() {
  const { slug } = useLocalSearchParams<{ slug: string }>();

  return <TopicScreen slug={slug} />;
}
