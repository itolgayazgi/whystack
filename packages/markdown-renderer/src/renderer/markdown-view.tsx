// biome-ignore-all lint/suspicious/noArrayIndexKey: This tree is derived from a string. It is replaced
// wholesale when the topic or the language changes, nothing under it holds state, and no item ever moves.
// The rule exists to stop a REORDERING list from reusing the wrong component and losing somebody's cursor,
// scroll or half-typed input — none of which exists here. A synthetic id would be invented per render and
// would be strictly worse: it would change every time and defeat reconciliation entirely.

import { type Palette, radius, reading, space, type TextToken } from '@whystack/theme';
import type { ReactNode } from 'react';
import { Pressable, ScrollView, StyleSheet, Text, type TextStyle, View } from 'react-native';
import type { Block, CalloutKind, Inline } from '../parser/nodes';

/**
 * Renders the parsed tree with design tokens, and nothing else.
 *
 * <b>The theme arrives as a prop.</b> This package cannot import the app's `useTheme` — `packages/*` may
 * not depend on `apps/*` (`06` Boundary Rules), and a renderer that reached into application state could
 * never be used by the static site either. So the app, which owns the hook, hands the palette in.
 *
 * There is not one literal colour, size or radius below. Every value comes from `packages/theme`
 * (CLAUDE.md §1.8), which is also what makes the reading font scale work: change it in Settings and this
 * whole tree re-renders at the new size, because the size was never written here.
 */
export interface MarkdownTheme {
  color: Palette;

  /**
   * The app's resolver, handed in — NOT the raw token.
   *
   * `packages/theme`'s TextStyle is a token DEFINITION (role, size per density, weight). What a component
   * needs is a ready-to-spread React Native style at the CURRENT density and reading font scale, and only
   * the app knows those: they come from the window width and from the reader's settings. So the app passes
   * its resolver, and this package renders whatever it is given.
   */
  textStyle: (token: TextToken) => TextStyle;
}

export interface MarkdownViewProps {
  blocks: Block[];
  theme: MarkdownTheme;
  /** An internal link (`/topics/…`) was tapped. The app owns navigation; this component does not. */
  onTopicPress?: (slug: string) => void;
}

/**
 * Index keys, everywhere below, and deliberately.
 *
 * The rule exists because a React list that REORDERS with index keys reuses the wrong component and moves
 * a user's cursor, their scroll, their half-typed input. None of that applies here: this tree is derived
 * from a string, it is replaced wholesale when the topic or the language changes, nothing under it holds
 * state, and no item ever moves. A synthetic id would be invented per render and would be strictly worse —
 * it would change every time and defeat reconciliation entirely.
 */
export function MarkdownView({ blocks, theme, onTopicPress }: MarkdownViewProps) {
  return (
    <View>
      {blocks.map((block, index) => (
        <BlockView key={index} block={block} theme={theme} onTopicPress={onTopicPress} first={index === 0} />
      ))}
    </View>
  );
}

function BlockView({
  block,
  theme,
  onTopicPress,
  first = false,
}: {
  block: Block;
  theme: MarkdownTheme;
  onTopicPress?: (slug: string) => void;
  first?: boolean;
}): ReactNode {
  const { color, textStyle } = theme;

  switch (block.type) {
    case 'paragraph':
      return (
        <Text style={[textStyle('body'), { marginTop: first ? 0 : reading.paragraphSpacing }]}>
          <Inlines inlines={block.children} theme={theme} onTopicPress={onTopicPress} />
        </Text>
      );

    case 'heading':
      return (
        <Text
          role="heading"
          aria-level={block.level}
          style={[
            textStyle(block.level === 3 ? 'sectionTitle' : 'subsectionTitle'),
            {
              color: color.textPrimary,
              // A heading sits CLOSE to the paragraph below it and FAR from the one above. Space it evenly
              // and it belongs to neither — which is how a reader loses track of what a section covers.
              marginTop: first ? 0 : reading.headingTopSpacing,
              marginBottom: reading.headingBottomSpacing,
            },
          ]}
        >
          <Inlines inlines={block.children} theme={theme} onTopicPress={onTopicPress} />
        </Text>
      );

    case 'code':
      return <CodeBlockView language={block.language} code={block.code} theme={theme} first={first} />;

    case 'list':
      return (
        <View style={{ marginTop: first ? 0 : reading.paragraphSpacing, gap: space[8] }}>
          {block.items.map((item, index) => (
            <View key={index} style={{ flexDirection: 'row', gap: space[8] }}>
              <Text style={[textStyle('body'), { color: color.textMuted }]}>
                {block.ordered ? `${index + 1}.` : '•'}
              </Text>

              <View style={{ flex: 1 }}>
                {item.map((child, childIndex) => (
                  <BlockView
                    key={childIndex}
                    block={child}
                    theme={theme}
                    onTopicPress={onTopicPress}
                    first={childIndex === 0}
                  />
                ))}
              </View>
            </View>
          ))}
        </View>
      );

    case 'table':
      return <TableView table={block} theme={theme} onTopicPress={onTopicPress} first={first} />;

    case 'callout':
      return (
        <CalloutView
          kind={block.kind}
          title={block.title}
          theme={theme}
          first={first}
          onTopicPress={onTopicPress}
        >
          {block.children}
        </CalloutView>
      );

    case 'quote':
      return (
        <View
          style={{
            marginTop: first ? 0 : reading.paragraphSpacing,
            paddingLeft: space[16],
            borderLeftWidth: 3,
            borderLeftColor: color.borderInteractive,
          }}
        >
          {block.children.map((child, index) => (
            <BlockView
              key={index}
              block={child}
              theme={theme}
              onTopicPress={onTopicPress}
              first={index === 0}
            />
          ))}
        </View>
      );

    case 'divider':
      return (
        <View
          style={{
            marginVertical: reading.sectionSpacing / 2,
            height: StyleSheet.hairlineWidth,
            backgroundColor: color.border,
          }}
        />
      );
  }
}

/**
 * A code block, and the one thing it must never do: wrap.
 *
 * Code is not prose. A wrapped line changes what the code MEANS to a reader learning the syntax — an
 * indentation level appears where there is none, and a long string looks like two. So it scrolls
 * horizontally, which is the only honest way to show a line that is wider than the screen.
 */
function CodeBlockView({
  language,
  code,
  theme,
  first,
}: {
  language: string;
  code: string;
  theme: MarkdownTheme;
  first: boolean;
}) {
  const { color, textStyle } = theme;

  return (
    <View
      style={{
        marginTop: first ? 0 : reading.paragraphSpacing,
        borderRadius: radius.medium,
        backgroundColor: color.codeBackground,
        borderWidth: 1,
        borderColor: color.border,
        overflow: 'hidden',
      }}
    >
      {language.length > 0 ? (
        <Text
          style={[
            textStyle('caption'),
            {
              color: color.textMuted,
              paddingHorizontal: space[12],
              paddingTop: space[8],
            },
          ]}
        >
          {language}
        </Text>
      ) : null}

      <ScrollView
        horizontal
        showsHorizontalScrollIndicator={false}
        contentContainerStyle={{ padding: space[12] }}
      >
        <Text selectable style={[textStyle('code'), { color: color.codeText }]}>
          {code}
        </Text>
      </ScrollView>
    </View>
  );
}

/**
 * A table, which scrolls sideways rather than squeezing.
 *
 * `10` requires tables, and a phone is 390 points wide. Squeezing a three-column comparison into that
 * produces one word per line and a table nobody can read — which is worse than making them swipe, because
 * a swipe is a gesture people already know.
 */
function TableView({
  table,
  theme,
  onTopicPress,
  first,
}: {
  table: Extract<Block, { type: 'table' }>;
  theme: MarkdownTheme;
  onTopicPress?: (slug: string) => void;
  first: boolean;
}) {
  const { color, textStyle } = theme;

  const cell = (inlines: Inline[], key: number, header: boolean) => (
    <View
      key={key}
      style={{
        minWidth: 140,
        flex: 1,
        padding: space[12],
        borderRightWidth: StyleSheet.hairlineWidth,
        borderRightColor: color.border,
      }}
    >
      <Text style={[textStyle(header ? 'label' : 'bodySmall'), header ? { color: color.textPrimary } : null]}>
        <Inlines inlines={inlines} theme={theme} onTopicPress={onTopicPress} />
      </Text>
    </View>
  );

  return (
    <ScrollView
      horizontal
      showsHorizontalScrollIndicator={false}
      style={{ marginTop: first ? 0 : reading.paragraphSpacing }}
    >
      <View
        style={{
          borderWidth: 1,
          borderColor: color.border,
          borderRadius: radius.small,
          overflow: 'hidden',
        }}
      >
        <View style={{ flexDirection: 'row', backgroundColor: color.surfaceMuted }}>
          {table.header.map((header, index) => cell(header, index, true))}
        </View>

        {table.rows.map((row, rowIndex) => (
          <View
            key={rowIndex}
            style={{
              flexDirection: 'row',
              borderTopWidth: StyleSheet.hairlineWidth,
              borderTopColor: color.border,
            }}
          >
            {row.map((column, columnIndex) => cell(column, columnIndex, false))}
          </View>
        ))}
      </View>
    </ScrollView>
  );
}

/**
 * A callout, and its colour is never the only thing that says what it is.
 *
 * `09` Forbidden Pattern 06: state must not be carried by colour alone. A colour-blind reader sees a
 * warning and a note as the same grey box. So each one also carries a WORD — and the word is the accessible
 * label a screen reader announces, not decoration.
 */
const CALLOUT_TONE: Record<CalloutKind, { color: keyof Palette; label: string }> = {
  note: { color: 'info', label: 'Note' },
  warning: { color: 'warning', label: 'Warning' },
  'best-practice': { color: 'success', label: 'Best practice' },
  version: { color: 'deprecated', label: 'Version' },
};

function CalloutView({
  kind,
  title,
  theme,
  first,
  onTopicPress,
  children,
}: {
  kind: CalloutKind;
  title?: string;
  theme: MarkdownTheme;
  first: boolean;
  onTopicPress?: (slug: string) => void;
  children: Block[];
}) {
  const { color, textStyle } = theme;
  const tone = CALLOUT_TONE[kind];
  const accent = color[tone.color];

  return (
    <View
      role="note"
      aria-label={title ?? tone.label}
      style={{
        marginTop: first ? 0 : reading.paragraphSpacing,
        padding: space[16],
        borderRadius: radius.medium,
        borderLeftWidth: 4,
        borderLeftColor: accent,
        backgroundColor: color.surfaceMuted,
      }}
    >
      <Text style={[textStyle('label'), { color: accent, marginBottom: space[8] }]}>
        {title ?? tone.label}
      </Text>

      {children.map((child, index) => (
        <BlockView key={index} block={child} theme={theme} onTopicPress={onTopicPress} first={index === 0} />
      ))}
    </View>
  );
}

function Inlines({
  inlines,
  theme,
  onTopicPress,
}: {
  inlines: Inline[];
  theme: MarkdownTheme;
  onTopicPress?: (slug: string) => void;
}) {
  const { color, textStyle } = theme;

  return (
    <>
      {inlines.map((inline, index) => {
        switch (inline.type) {
          case 'text': {
            const style: TextStyle = {};
            if (inline.strong) style.fontWeight = '600';
            if (inline.emphasis) style.fontStyle = 'italic';

            return (
              <Text key={index} style={style}>
                {inline.text}
              </Text>
            );
          }

          case 'inline-code':
            // Inline code sits INSIDE a line of prose, so it cannot have its own line height — that would
            // push the whole paragraph's leading apart wherever a `List<T>` appears. It gets the code face
            // and the code colours, and nothing that changes the line box.
            return (
              <Text
                key={index}
                style={[
                  textStyle('code'),
                  {
                    color: color.codeText,
                    backgroundColor: color.codeBackground,
                    lineHeight: undefined,
                  },
                ]}
              >
                {inline.code}
              </Text>
            );

          case 'link': {
            const label = <Inlines inlines={inline.children} theme={theme} onTopicPress={onTopicPress} />;

            if (inline.internal && inline.slug !== undefined && onTopicPress) {
              const slug = inline.slug;

              return (
                <Pressable
                  key={index}
                  accessibilityRole="link"
                  onPress={() => onTopicPress(slug)}
                  // The link is underlined AND coloured. Colour alone fails `09` Forbidden Pattern 06, and
                  // it is exactly the case that matters: a reader who cannot see the accent cannot find the
                  // one word on the page that leads somewhere.
                >
                  <Text style={[textStyle('body'), { color: color.accent, textDecorationLine: 'underline' }]}>
                    {label}
                  </Text>
                </Pressable>
              );
            }

            return (
              <Text
                key={index}
                accessibilityRole="link"
                style={{ color: color.accent, textDecorationLine: 'underline' }}
              >
                {label}
              </Text>
            );
          }

          default:
            // Unreachable: Inline is a closed union and every member is handled above. Present because a
            // callback that can fall off the end returns undefined, and React would render nothing while
            // saying nothing — the exact silence this codebase does not allow.
            return null;
        }
      })}
    </>
  );
}
