import type { Block, Inline } from '../parser/nodes';

/**
 * The WEB renderer: the same parse tree, rendered to semantic HTML.
 *
 * The sibling of `./native`. Neither knows the other exists — they agree only on `parser/nodes`, which is
 * the whole reason that tree is ours rather than markdown-it's. One parse, two platforms (ADR-0022).
 *
 * It emits plain elements with class names and NO inline styles: the page that hosts it owns the look
 * through CSS, so a topic inside the reading screen and a topic inside the studio preview can differ without
 * this file knowing either exists.
 */

export interface MarkdownProps {
  blocks: Block[];

  /** Class prefix, so a host can scope its CSS. Defaults to `md`. */
  prefix?: string;

  /** Internal links are navigation. The host decides how — Next's Link, a plain anchor, a preview no-op. */
  renderLink?: (slug: string, children: React.ReactNode) => React.ReactNode;
}

export function Markdown({ blocks, prefix = 'md', renderLink }: MarkdownProps) {
  return <>{blocks.map((block, index) => renderBlock(block, `${index}`, prefix, renderLink))}</>;
}

function renderBlock(
  block: Block,
  key: string,
  prefix: string,
  renderLink: MarkdownProps['renderLink'],
): React.ReactNode {
  switch (block.type) {
    case 'paragraph':
      return (
        <p key={key} className={`${prefix}-p`}>
          {block.children.map((inline, index) => renderInline(inline, `${key}-${index}`, prefix, renderLink))}
        </p>
      );

    case 'heading': {
      // `##` never reaches here — it names the section and is consumed upstream. So the smallest heading a
      // body can carry is `###`, and it maps to <h3>, keeping the page's outline honest.
      const Tag = `h${block.level}` as 'h3' | 'h4' | 'h5' | 'h6';

      return (
        <Tag key={key} className={`${prefix}-h ${prefix}-h${block.level}`}>
          {block.children.map((inline, index) => renderInline(inline, `${key}-${index}`, prefix, renderLink))}
        </Tag>
      );
    }

    case 'list': {
      const Tag = block.ordered ? 'ol' : 'ul';

      return (
        <Tag key={key} className={`${prefix}-list`}>
          {block.items.map((item, index) => (
            <li key={`${key}-${index}`} className={`${prefix}-li`}>
              {item.map((child, childIndex) =>
                renderBlock(child, `${key}-${index}-${childIndex}`, prefix, renderLink),
              )}
            </li>
          ))}
        </Tag>
      );
    }

    case 'code':
      return (
        <pre key={key} className={`${prefix}-pre`} data-language={block.language || undefined}>
          <code className={`${prefix}-code`}>{block.code}</code>
        </pre>
      );

    case 'table':
      // Wrapped in its own scroll container. A wide table must scroll INSIDE itself — a page that scrolls
      // sideways because of one table is a page nobody can read on a phone.
      return (
        <div key={key} className={`${prefix}-table-scroll`}>
          <table className={`${prefix}-table`}>
            <thead>
              <tr>
                {block.header.map((cell, index) => (
                  <th key={`${key}-h-${index}`}>
                    {cell.map((inline, i) =>
                      renderInline(inline, `${key}-h-${index}-${i}`, prefix, renderLink),
                    )}
                  </th>
                ))}
              </tr>
            </thead>
            <tbody>
              {block.rows.map((row, rowIndex) => (
                <tr key={`${key}-r-${rowIndex}`}>
                  {row.map((cell, cellIndex) => (
                    <td key={`${key}-r-${rowIndex}-${cellIndex}`}>
                      {cell.map((inline, i) =>
                        renderInline(inline, `${key}-r-${rowIndex}-${cellIndex}-${i}`, prefix, renderLink),
                      )}
                    </td>
                  ))}
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      );

    case 'callout':
      return (
        <aside key={key} className={`${prefix}-callout ${prefix}-callout-${block.kind}`}>
          {block.title ? <p className={`${prefix}-callout-title`}>{block.title}</p> : null}
          {block.children.map((child, index) => renderBlock(child, `${key}-${index}`, prefix, renderLink))}
        </aside>
      );

    case 'quote':
      return (
        <blockquote key={key} className={`${prefix}-quote`}>
          {block.children.map((child, index) => renderBlock(child, `${key}-${index}`, prefix, renderLink))}
        </blockquote>
      );

    case 'divider':
      return <hr key={key} className={`${prefix}-hr`} />;

    default: {
      // Exhaustive: a new node type fails to compile here rather than rendering nothing at runtime.
      const never: never = block;
      return never;
    }
  }
}

function renderInline(
  inline: Inline,
  key: string,
  prefix: string,
  renderLink: MarkdownProps['renderLink'],
): React.ReactNode {
  switch (inline.type) {
    case 'text': {
      let node: React.ReactNode = inline.text;

      if (inline.emphasis) node = <em key={`${key}-em`}>{node}</em>;
      if (inline.strong) node = <strong key={`${key}-strong`}>{node}</strong>;

      return <span key={key}>{node}</span>;
    }

    case 'inline-code':
      return (
        <code key={key} className={`${prefix}-inline-code`}>
          {inline.code}
        </code>
      );

    case 'link': {
      const children = inline.children.map((child, index) =>
        renderInline(child, `${key}-${index}`, prefix, renderLink),
      );

      // Internal links are navigation and the host owns how (ADR-0022: the website routes them, the app
      // pushes a screen). External ones leave, and carry the usual protection on a target=_blank.
      if (inline.internal && inline.slug && renderLink) {
        return <span key={key}>{renderLink(inline.slug, children)}</span>;
      }

      return (
        <a
          key={key}
          className={`${prefix}-link`}
          href={inline.href}
          {...(inline.internal ? {} : { target: '_blank', rel: 'noopener noreferrer' })}
        >
          {children}
        </a>
      );
    }

    default: {
      const never: never = inline;
      return never;
    }
  }
}
