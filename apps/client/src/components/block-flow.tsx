import type { TopicBlock } from '@whystack/api-client';
import { parse } from '@whystack/markdown-renderer';
import { MarkdownView } from '@whystack/markdown-renderer/native';
import { fontFamily } from '@whystack/theme';
import { Component, type ReactNode, useMemo, useState } from 'react';
import { Pressable, StyleSheet, Text, View } from 'react-native';
import { SvgXml } from 'react-native-svg';
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
}: {
  blocks: TopicBlock[];
  onTopicPress?: (slug: string) => void;
  onBlockLayout?: (order: number, y: number) => void;
}) {
  return (
    <>
      {blocks.map((block) => (
        <View
          key={`${block.order}-${block.type}`}
          onLayout={(event) => onBlockLayout?.(block.order, event.nativeEvent.layout.y)}
        >
          <BlockTag label={LABEL[block.type]} />
          <BlockBody block={block} onTopicPress={onTopicPress} />
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

function BlockBody({ block, onTopicPress }: { block: TopicBlock; onTopicPress?: (slug: string) => void }) {
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
      return <Checkpoint data={block.data} />;

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
function Checkpoint({ data }: { data: Extract<TopicBlock, { type: 'Checkpoint' }>['data'] }) {
  const { color } = useTheme();
  const [chosen, setChosen] = useState<number | null>(null);

  const answered = chosen !== null;

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
        const isCorrect = index === data.correctIndex;
        const border = !answered
          ? color.border
          : isCorrect
            ? color.success
            : index === chosen
              ? color.error
              : color.border;

        return (
          <Pressable
            key={option}
            disabled={answered}
            onPress={() => setChosen(index)}
            style={[styles.option, { borderColor: border }]}
          >
            <Text style={[styles.optionKey, { color: color.textMuted, fontFamily: fontFamily.code }]}>
              [{String.fromCharCode(97 + index)}]
            </Text>
            <Text style={[styles.optionText, { color: color.textSecondary }]}>{option}</Text>
          </Pressable>
        );
      })}

      {answered ? (
        <View style={[styles.why, { borderLeftColor: color.success }]}>
          <Text style={[styles.whyLead, { color: color.success }]}>
            {chosen === data.correctIndex ? 'Doğru. ' : 'Değil. '}
          </Text>
          <Text style={[styles.whyText, { color: color.textSecondary }]}>{data.explanation}</Text>
        </View>
      ) : null}
    </View>
  );
}

const styles = StyleSheet.create({
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
