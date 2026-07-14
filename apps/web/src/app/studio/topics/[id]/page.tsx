'use client';

import { useParams } from 'next/navigation';
import { TopicEditor } from '@/components/studio/topic-editor';

export default function EditTopicPage() {
  const { id } = useParams<{ id: string }>();

  return <TopicEditor topicId={id} />;
}
