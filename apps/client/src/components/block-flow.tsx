import type { TopicBlock } from '@whystack/api-client';
import { parse } from '@whystack/markdown-renderer';
import { MarkdownView } from '@whystack/markdown-renderer/native';
import { fontFamily } from '@whystack/theme';
import { Component, type ReactNode, useCallback, useMemo, useRef, useState } from 'react';
import { Pressable, StyleSheet, Text, View } from 'react-native';
import { SvgXml } from 'react-native-svg';
import { useLanguage } from '../state/language';
import { useTheme } from '../state/theme';

/**
 * The block flow on a phone (ADR-0024) — the same JSON the website renders.
 *
 * <b>A topic is not SHORTENED on mobile; it is broken into blocks.</b> Same depth, smaller bites. The web
 * reader gets one column and a scrollspy; here each block is its own card and the bar at the top fills as you
 * pass them. Neither surface re-derives anything — the API already merged the shared blocks with the chosen
 * ecosystem's.
 */

const LABEL: Record<TopicBlock['type'], string> = {
  Hook: 'Kanca',
  Story: 'Problem hikâyesi',
  Concept: 'Kavram — zihinsel model',
  Code: 'Kod',
  Diagram: 'Diyagram',
  Compare: 'Karşılaştırma',
  Myth: 'Yaygın yanılgı',
  Checkpoint: 'Checkpoint',
  Prod: 'Production notu',
  Term: 'Terim',
  Summary: 'Özet',
  Next: 'Sonraki durak',
};

export function BlockFlow({
  blocks,
  onTopicPress,
  onBlockLayout,
  onAllCheckpointsPassed,
}: {
  blocks: TopicBlock[];
  onTopicPress?: (slug: string) => void;
  onBlockLayout?: (order: number, y: number) => void;

  /**
   * EVERY checkpoint, not the first. The message under a wrong answer promises "bir sonraki konuya hazır
   * olduğundan emin olmalıyım", and we cannot be sure of that while one of them is still wrong.
   */
  onAllCheckpointsPassed?: () => void;
}) {
  const total = blocks.filter((block) => block.type === 'Checkpoint').length;

  // A ref, not state: nothing on screen depends on WHICH ones are passed, only on the moment the last one
  // is. State would re-render every block in the flow on each correct answer.
  const passed = useRef<Set<number>>(new Set());

  const onCorrect = useCallback(
    (order: number) => {
      passed.current.add(order);
      if (passed.current.size === total) onAllCheckpointsPassed?.();
    },
    [total, onAllCheckpointsPassed],
  );

  return (
    <>
      {blocks.map((block) => (
        <View
          key={`${block.order}-${block.type}`}
          onLayout={(event) => onBlockLayout?.(block.order, event.nativeEvent.layout.y)}
        >
          <BlockTag label={LABEL[block.type]} />
          <BlockBody block={block} onTopicPress={onTopicPress} onCorrect={onCorrect} />
        </View>
      ))}
    </>
  );
}

function BlockTag({ label }: { label: string }) {
  const { color } = useTheme();

  return (
    <View style={styles.tagRow}>
      <View style={[styles.tagRule, { backgroundColor: color.border }]} />
      <Text style={[styles.tagText, { color: color.textMuted, fontFamily: fontFamily.code }]}>
        {label.toLocaleUpperCase('tr')}
      </Text>
    </View>
  );
}

function BlockBody({
  block,
  onTopicPress,
  onCorrect,
}: {
  block: TopicBlock;
  onTopicPress?: (slug: string) => void;
  onCorrect: (order: number) => void;
}) {
  const { color } = useTheme();

  switch (block.type) {
    case 'Hook':
      return (
        <View style={[styles.hook, { backgroundColor: color.surface, borderColor: color.accent }]}>
          {/* A question, never a definition (ADR-0019). */}
          <Text style={[styles.hookQuestion, { color: color.textPrimary, fontFamily: fontFamily.display }]}>
            {block.data.question}
          </Text>
          {block.data.promise ? (
            <Text style={[styles.hookPromise, { color: color.textSecondary }]}>{block.data.promise}</Text>
          ) : null}
        </View>
      );

    case 'Story':
    case 'Concept':
      return (
        <View style={styles.block}>
          {block.data.analogy ? (
            <Text style={[styles.analogy, { color: color.textSecondary, borderLeftColor: color.accent }]}>
              {block.data.analogy}
            </Text>
          ) : null}
          <Prose markdown={block.data.markdown} onTopicPress={onTopicPress} />
        </View>
      );

    case 'Prod':
      return (
        <View style={[styles.prod, { backgroundColor: color.surfaceElevated, borderLeftColor: color.info }]}>
          <Text style={[styles.prodTitle, { color: color.info, fontFamily: fontFamily.display }]}>
            SAHADA NEREDE PATLAR?
          </Text>
          <Prose markdown={block.data.markdown} onTopicPress={onTopicPress} />
        </View>
      );

    case 'Code':
      return <CodeBlock data={block.data} />;

    case 'Myth':
      return (
        <View style={[styles.myth, { borderColor: color.error }]}>
          <Text style={[styles.mythClaim, { color: color.error }]}>⚠️ {block.data.claim}</Text>
          <Text style={[styles.mythTruth, { color: color.textSecondary }]}>{block.data.truth}</Text>
        </View>
      );

    case 'Checkpoint':
      return <Checkpoint data={block.data} onCorrect={() => onCorrect(block.order)} />;

    case 'Term':
      return (
        <View style={[styles.term, { backgroundColor: color.surface, borderColor: color.border }]}>
          <Text style={[styles.termName, { color: color.accent, fontFamily: fontFamily.code }]}>
            {block.data.term}
          </Text>
          <Text style={[styles.termDefinition, { color: color.textSecondary }]}>{block.data.definition}</Text>
        </View>
      );

    case 'Summary':
      return (
        <View style={[styles.summary, { backgroundColor: color.surface, borderColor: color.accent }]}>
          <Text style={[styles.summaryTitle, { color: color.accent, fontFamily: fontFamily.display }]}>
            Bu duraktan cebinde kalanlar
          </Text>
          {block.data.items.map((item) => (
            <View key={item} style={styles.summaryRow}>
              <Text style={[styles.summaryBullet, { color: color.accent }]}>▸</Text>
              <Text style={[styles.summaryItem, { color: color.textSecondary }]}>{item}</Text>
            </View>
          ))}
        </View>
      );

    case 'Next':
      return (
        <View style={[styles.next, { borderColor: color.accent }]}>
          <Text style={[styles.nextLabel, { color: color.textPrimary }]}>{block.data.label}</Text>
          <Text style={[styles.nextMeta, { color: color.textSecondary }]}>Bu durağın devamı</Text>
        </View>
      );

    case 'Compare':
      // A table on a 375pt screen is a table nobody reads. The design's answer is card-per-row; until that
      // exists it is honest to say so rather than render an unreadable grid.
      return (
        <View style={[styles.block, { borderColor: color.border }]}>
          <Text style={{ color: color.textMuted }}>
            Karşılaştırma tablosu mobilde henüz kart olarak gösterilmiyor.
          </Text>
        </View>
      );

    case 'Diagram':
      return <Diagram data={block.data} />;

    default: {
      const never: never = block;
      return never;
    }
  }
}

function Prose({ markdown, onTopicPress }: { markdown: string; onTopicPress?: (slug: string) => void }) {
  const { color, textStyle } = useTheme();

  // Parsed once per body. The tree comes purely from the Markdown, which does not change while the screen is
  // open — reparsing on a theme toggle would be work nobody asked for, on the screen a reader sits longest on.
  const tree = useMemo(() => parse(markdown), [markdown]);

  return <MarkdownView blocks={tree} theme={{ color, textStyle }} onTopicPress={onTopicPress} />;
}

/**
 * A diagram — the authored SVG, rendered natively.
 *
 * `SvgXml` parses the markup at runtime, which is why the diagram is one string in the block's data rather
 * than a component: the same string the website drops into the DOM (ADR-0022 — one JSON, two platforms).
 *
 * A malformed SVG throws inside the parser, and a diagram must not take the topic down around it — so it is
 * caught, and the caption is shown instead. Silence would leave a hole nobody could explain.
 */
function Diagram({ data }: { data: Extract<TopicBlock, { type: 'Diagram' }>['data'] }) {
  const { color } = useTheme();

  return (
    <View style={[styles.diagram, { backgroundColor: color.surface, borderColor: color.border }]}>
      <SvgBoundary
        fallback={
          <Text style={{ color: color.textMuted }}>{data.caption ?? 'Bu diyagram gösterilemedi.'}</Text>
        }
      >
        <SvgXml xml={data.svg} width="100%" />
      </SvgBoundary>

      {data.caption ? <Text style={[styles.caption, { color: color.textMuted }]}>{data.caption}</Text> : null}
    </View>
  );
}

/**
 * A real error boundary, because a try/catch around JSX is not one.
 *
 * `SvgXml` parses the markup DURING RENDER, so a malformed diagram throws inside React's render pass — where
 * a try/catch in the parent never sees it, and where an uncaught throw unmounts the whole topic. Only a class
 * component with `getDerivedStateFromError` can stop it at this block.
 */
class SvgBoundary extends Component<{ children: ReactNode; fallback: ReactNode }, { failed: boolean }> {
  state = { failed: false };

  static getDerivedStateFromError() {
    return { failed: true };
  }

  render() {
    return this.state.failed ? this.props.fallback : this.props.children;
  }
}

function CodeBlock({ data }: { data: Extract<TopicBlock, { type: 'Code' }>['data'] }) {
  const { color } = useTheme();

  return (
    <View style={[styles.code, { backgroundColor: color.codeBackground, borderColor: color.border }]}>
      <View style={[styles.codeHead, { backgroundColor: color.surface, borderBottomColor: color.border }]}>
        <Text style={[styles.codeFile, { color: color.textMuted, fontFamily: fontFamily.code }]}>
          {data.file ?? data.lang}
        </Text>
      </View>

      <Text style={[styles.codeSource, { color: color.codeText, fontFamily: fontFamily.code }]}>
        {data.source}
      </Text>

      {data.annotation ? (
        <Text
          style={[
            styles.codeAnnotation,
            { backgroundColor: color.surface, borderTopColor: color.border, color: color.textSecondary },
          ]}
        >
          {data.annotation}
        </Text>
      ) : null}
    </View>
  );
}

/**
 * The checkpoint. Reveals the explanation on answer — right or wrong.
 *
 * A checkpoint that only scores teaches nothing; the explanation IS the block, which is why the schema makes
 * it mandatory and why it appears either way.
 */
/**
 * The checkpoint — the block that breaks passive reading, and the one that says a topic is finished.
 *
 * The same rules as the website, for the same reasons (see apps/web block-flow): a wrong answer does not
 * reveal the correct option, the retry is unlimited and kindly worded, and the explanation is held back
 * until the reader gets there. Two platforms, one behaviour — because it is one product decision, not a
 * per-platform detail.
 */
function Checkpoint({
  data,
  onCorrect,
}: {
  data: Extract<TopicBlock, { type: 'Checkpoint' }>['data'];
  onCorrect: () => void;
}) {
  const { color } = useTheme();
  const { t } = useLanguage();
  const [chosen, setChosen] = useState<number | null>(null);

  const answered = chosen !== null;
  const correct = chosen === data.correctIndex;

  return (
    <View
      style={[
        styles.checkpoint,
        { backgroundColor: color.surface, borderColor: color.border, borderTopColor: color.accent },
      ]}
    >
      <Text style={[styles.checkpointLabel, { color: color.accent, fontFamily: fontFamily.display }]}>
        {'// Kendi cümlenle doğrula'}
      </Text>
      <Text style={[styles.checkpointQuestion, { color: color.textPrimary }]}>{data.question}</Text>

      {data.options.map((option, index) => {
        // The correct one is marked ONLY once they have found it. Marking it on a wrong answer would put the
        // answer on screen and make the retry below theatre.
        const border = !answered
          ? color.border
          : correct && index === data.correctIndex
            ? color.success
            : index === chosen
              ? color.accent
              : color.border;

        return (
          <Pressable
            key={option}
            testID={`option-${index}`}
            accessibilityRole="button"
            // Locked once they are right; left live while they are wrong, because the next thing they should
            // be able to do is think again.
            disabled={correct}
            onPress={() => {
              setChosen(index);
              if (index === data.correctIndex) onCorrect();
            }}
            style={[styles.option, { borderColor: border }]}
          >
            <Text style={[styles.optionKey, { color: color.textMuted, fontFamily: fontFamily.code }]}>
              [{String.fromCharCode(97 + index)}]
            </Text>
            <Text style={[styles.optionText, { color: color.textSecondary }]}>{option}</Text>
          </Pressable>
        );
      })}

      {answered && correct ? (
        <View style={[styles.why, { borderLeftColor: color.success }]}>
          <Text style={[styles.whyLead, { color: color.success }]}>{t('checkpoint.correct')} </Text>
          <Text style={[styles.whyText, { color: color.textSecondary }]}>{data.explanation}</Text>
        </View>
      ) : null}

      {answered && !correct ? (
        /*
          Deliberately NOT the error colour. This is not a failure — it is a person thinking, and a red box
          is a poor answer to thinking. The accent says "you are still here"; the error palette would say
          "you did something wrong", which is the message the owner asked us not to send.
        */
        <View testID="checkpoint-retry" role="status" style={[styles.why, { borderLeftColor: color.accent }]}>
          <Text style={[styles.whyText, { color: color.textPrimary }]}>{t('checkpoint.tryAgain.lead')}</Text>

          <Pressable
            testID="checkpoint-retry-button"
            accessibilityRole="button"
            onPress={() => setChosen(null)}
            style={[styles.retryButton, { backgroundColor: color.accent }]}
          >
            <Text style={[styles.retryButtonText, { color: color.background }]}>
              {t('checkpoint.tryAgain.button')}
            </Text>
          </Pressable>

          <Text style={[styles.retryHint, { color: color.textMuted }]}>{t('checkpoint.tryAgain.hint')}</Text>
        </View>
      ) : null}
    </View>
  );
}

const styles = StyleSheet.create({
  retryButton: {
    minHeight: 44,
    justifyContent: 'center',
    alignItems: 'center',
    borderRadius: 6,
    marginTop: 12,
    paddingHorizontal: 16,
  },
  retryButtonText: { fontSize: 13, fontWeight: '700' },
  retryHint: { fontSize: 11.5, marginTop: 8, lineHeight: 16 },

  tagRow: { flexDirection: 'row', alignItems: 'center', gap: 8, marginBottom: 8, marginTop: 18 },
  tagRule: { width: 18, height: 1 },
  tagText: { fontSize: 9.5, letterSpacing: 1.4 },

  block: { marginBottom: 4 },

  hook: { borderWidth: 1, borderRadius: 12, padding: 16 },
  hookQuestion: { fontSize: 15.5, fontWeight: '600', lineHeight: 22 },
  hookPromise: { fontSize: 12.5, lineHeight: 19, marginTop: 8 },

  analogy: { borderLeftWidth: 2, paddingLeft: 10, fontStyle: 'italic', marginBottom: 10, fontSize: 13.5 },

  code: { borderWidth: 1, borderRadius: 10, overflow: 'hidden' },
  codeHead: { paddingVertical: 7, paddingHorizontal: 13, borderBottomWidth: 1 },
  codeFile: { fontSize: 10.5 },
  codeSource: { padding: 13, fontSize: 11.5, lineHeight: 20 },
  codeAnnotation: { borderTopWidth: 1, padding: 12, fontSize: 11.5, lineHeight: 18 },

  myth: { borderWidth: 1, borderRadius: 12, padding: 14 },
  mythClaim: { fontSize: 13.5, fontWeight: '700', marginBottom: 6 },
  mythTruth: { fontSize: 13, lineHeight: 20 },

  checkpoint: { borderWidth: 1, borderTopWidth: 3, borderRadius: 12, padding: 16 },
  checkpointLabel: { fontSize: 10.5, letterSpacing: 1.2, marginBottom: 10 },
  checkpointQuestion: { fontSize: 13, lineHeight: 20, marginBottom: 12 },
  option: { flexDirection: 'row', gap: 8, borderWidth: 1, borderRadius: 8, padding: 11, marginBottom: 8 },
  optionKey: { fontSize: 11 },
  optionText: { fontSize: 12.5, flex: 1 },
  why: { borderLeftWidth: 3, paddingLeft: 11, paddingVertical: 8, marginTop: 4 },
  whyLead: { fontSize: 12.5, fontWeight: '600' },
  whyText: { fontSize: 12.5, lineHeight: 19 },

  prod: { borderLeftWidth: 4, borderRadius: 10, padding: 14 },
  prodTitle: { fontSize: 10.5, letterSpacing: 1, marginBottom: 8 },

  term: { borderWidth: 1, borderRadius: 8, padding: 12 },
  termName: { fontSize: 12.5 },
  termDefinition: { fontSize: 12.5, lineHeight: 19, marginTop: 4 },

  summary: { borderWidth: 1, borderRadius: 14, padding: 18 },
  summaryTitle: { fontSize: 14, marginBottom: 12 },
  summaryRow: { flexDirection: 'row', gap: 8, marginBottom: 6 },
  summaryBullet: { fontSize: 13 },
  summaryItem: { fontSize: 13, lineHeight: 21, flex: 1 },

  next: { borderWidth: 1, borderRadius: 10, padding: 14, marginTop: 12 },

  diagram: { borderWidth: 1, borderRadius: 10, padding: 14, alignItems: 'center' },
  caption: { fontSize: 11.5, marginTop: 10, textAlign: 'center' },
  nextLabel: { fontSize: 13.5, fontWeight: '600' },
  nextMeta: { fontSize: 11.5, marginTop: 2 },
});
