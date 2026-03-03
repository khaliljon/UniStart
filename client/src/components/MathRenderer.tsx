import { useMemo } from 'react';
import katex from 'katex';
import 'katex/dist/katex.min.css';

/**
 * Renders text with inline ($...$) and block ($$...$$) LaTeX math via KaTeX.
 * Also handles basic markdown: **bold**, *italic*, `code`.
 */
export function renderMath(text: string): string {
  if (!text) return '';

  // First render block math $$...$$
  let result = text.replace(/\$\$([\s\S]*?)\$\$/g, (_, math) => {
    try {
      return katex.renderToString(math.trim(), { displayMode: true, throwOnError: false });
    } catch {
      return `<pre class="math-error">${math}</pre>`;
    }
  });

  // Then render inline math $...$  (but not \$ escaped dollars)
  result = result.replace(/(?<![\\])\$([^$\n]+?)\$/g, (_, math) => {
    try {
      return katex.renderToString(math.trim(), { displayMode: false, throwOnError: false });
    } catch {
      return `<code class="math-error">${math}</code>`;
    }
  });

  return result;
}

/**
 * Renders simple markdown-like formatting to HTML.
 * Handles: **bold**, *italic*, `code`, $math$, $$block math$$
 */
export function renderMarkdown(text: string): string {
  if (!text) return '';

  let html = renderMath(text);

  // Bold **text**
  html = html.replace(/\*\*(.+?)\*\*/g, '<strong>$1</strong>');

  // Italic *text* (but not ** which is bold)
  html = html.replace(/(?<!\*)\*([^*]+?)\*(?!\*)/g, '<em>$1</em>');

  // Inline code `text`
  html = html.replace(/`(.+?)`/g,
    '<code style="background:rgba(99,102,241,0.1);padding:2px 6px;border-radius:3px;font-size:0.85em;font-family:monospace">$1</code>'
  );

  return html;
}

interface MathTextProps {
  text: string;
  className?: string;
  style?: React.CSSProperties;
  as?: 'span' | 'div' | 'p';
}

/**
 * Component that renders text with LaTeX math and basic markdown formatting.
 * Use for any text that may contain $...$ or $$...$$ math expressions.
 */
export default function MathText({ text, className, style, as: Tag = 'span' }: MathTextProps) {
  const html = useMemo(() => renderMarkdown(text), [text]);
  return <Tag className={className} style={style} dangerouslySetInnerHTML={{ __html: html }} />;
}

/**
 * Renders full lesson/guide content with proper structure.
 * Handles headers, lists, blockquotes, tables, math, and inline formatting.
 */
export function ContentRenderer({ content, style }: { content: string; style?: React.CSSProperties }) {
  const elements = useMemo(() => {
    const lines = content.split('\n');
    const result: React.ReactNode[] = [];
    let inTable = false;
    let tableRows: string[][] = [];

    const flushTable = () => {
      if (tableRows.length > 0) {
        result.push(
          <div key={`table-${result.length}`} style={{ overflowX: 'auto', margin: '1rem 0' }}>
            <table style={{ width: '100%', borderCollapse: 'collapse', fontSize: '0.875rem' }}>
              <thead>
                <tr>
                  {tableRows[0].map((cell, i) => (
                    <th key={i} style={{
                      padding: '0.5rem 0.75rem', textAlign: 'left',
                      borderBottom: '2px solid var(--border-color)', fontWeight: 600,
                      color: 'var(--text-primary)'
                    }}>
                      <span dangerouslySetInnerHTML={{ __html: renderMarkdown(cell) }} />
                    </th>
                  ))}
                </tr>
              </thead>
              <tbody>
                {tableRows.slice(1).map((row, ri) => (
                  <tr key={ri}>
                    {row.map((cell, ci) => (
                      <td key={ci} style={{
                        padding: '0.5rem 0.75rem',
                        borderBottom: '1px solid var(--border-color)',
                        color: 'var(--text-secondary)'
                      }}>
                        <span dangerouslySetInnerHTML={{ __html: renderMarkdown(cell) }} />
                      </td>
                    ))}
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        );
        tableRows = [];
      }
      inTable = false;
    };

    // Collect block math that spans multiple lines
    let i = 0;
    while (i < lines.length) {
      const line = lines[i];

      // Multi-line block math $$...$$
      if (line.trim() === '$$') {
        if (inTable) flushTable();
        const mathLines: string[] = [];
        i++;
        while (i < lines.length && lines[i].trim() !== '$$') {
          mathLines.push(lines[i]);
          i++;
        }
        const mathStr = mathLines.join('\n').trim();
        try {
          const html = katex.renderToString(mathStr, { displayMode: true, throwOnError: false });
          result.push(
            <div key={`bmath-${result.length}`} style={{ margin: '1rem 0', textAlign: 'center' }}
              dangerouslySetInnerHTML={{ __html: html }} />
          );
        } catch {
          result.push(
            <pre key={`bmath-${result.length}`} style={{
              margin: '0.75rem 0', padding: '0.75rem',
              backgroundColor: 'rgba(99, 102, 241, 0.05)', borderRadius: '0.5rem',
              fontFamily: 'monospace', fontSize: '0.875rem'
            }}>{mathStr}</pre>
          );
        }
        i++;
        continue;
      }

      // Single-line block math $$...$$
      if (line.trim().startsWith('$$') && line.trim().endsWith('$$') && line.trim().length > 4) {
        if (inTable) flushTable();
        const mathStr = line.trim().slice(2, -2).trim();
        try {
          const html = katex.renderToString(mathStr, { displayMode: true, throwOnError: false });
          result.push(
            <div key={`bmath-${result.length}`} style={{ margin: '1rem 0', textAlign: 'center' }}
              dangerouslySetInnerHTML={{ __html: html }} />
          );
        } catch {
          result.push(
            <pre key={`bmath-${result.length}`} style={{
              margin: '0.75rem 0', padding: '0.75rem',
              backgroundColor: 'rgba(99, 102, 241, 0.05)', borderRadius: '0.5rem',
              fontFamily: 'monospace', fontSize: '0.875rem'
            }}>{mathStr}</pre>
          );
        }
        i++;
        continue;
      }

      // Table row
      if (line.trim().startsWith('|') && line.trim().endsWith('|')) {
        // Skip separator rows like |---|---|
        if (line.match(/^\|[\s\-:|]+\|$/)) { i++; continue; }
        const cells = line.split('|').filter(c => c.trim() !== '').map(c => c.trim());
        tableRows.push(cells);
        inTable = true;
        i++;
        continue;
      }

      if (inTable) flushTable();

      // Headers
      if (line.startsWith('### ')) {
        result.push(
          <h4 key={i} style={{
            fontSize: '1rem', fontWeight: 600, color: 'var(--text-primary)',
            marginTop: '1.25rem', marginBottom: '0.375rem'
          }} dangerouslySetInnerHTML={{ __html: renderMarkdown(line.slice(4)) }} />
        );
      } else if (line.startsWith('## ')) {
        result.push(
          <h3 key={i} style={{
            fontSize: '1.125rem', fontWeight: 700, color: 'var(--text-primary)',
            marginTop: '1.5rem', marginBottom: '0.5rem'
          }} dangerouslySetInnerHTML={{ __html: renderMarkdown(line.slice(3)) }} />
        );
      }
      // Blockquote
      else if (line.startsWith('> ')) {
        result.push(
          <blockquote key={i} style={{
            margin: '0.75rem 0', padding: '0.75rem 1rem',
            borderLeft: '3px solid var(--primary-color)',
            backgroundColor: 'rgba(99, 102, 241, 0.05)',
            borderRadius: '0 0.5rem 0.5rem 0',
            fontSize: '0.875rem', color: 'var(--text-secondary)', fontStyle: 'italic'
          }} dangerouslySetInnerHTML={{ __html: renderMarkdown(line.slice(2)) }} />
        );
      }
      // Numbered list
      else if (line.match(/^(\d+)\. /)) {
        result.push(
          <p key={i} style={{
            margin: '0.25rem 0', paddingLeft: '1.5rem',
            fontSize: '0.875rem', color: 'var(--text-secondary)', lineHeight: 1.6
          }} dangerouslySetInnerHTML={{ __html: renderMarkdown(line) }} />
        );
      }
      // Bullet list
      else if (line.startsWith('- ')) {
        const content = line.slice(2);
        result.push(
          <p key={i} style={{
            margin: '0.25rem 0', paddingLeft: '1.5rem',
            fontSize: '0.875rem', color: 'var(--text-secondary)', lineHeight: 1.6
          }}>
            <span style={{ marginRight: '0.5rem' }}>&#8226;</span>
            <span dangerouslySetInnerHTML={{ __html: renderMarkdown(content) }} />
          </p>
        );
      }
      // Non-empty lines
      else if (line.trim().length > 0) {
        result.push(
          <p key={i} style={{
            margin: '0.375rem 0', fontSize: '0.875rem',
            color: 'var(--text-secondary)', lineHeight: 1.6
          }} dangerouslySetInnerHTML={{ __html: renderMarkdown(line) }} />
        );
      }
      // Empty line = spacer
      else {
        result.push(<div key={i} style={{ height: '0.5rem' }} />);
      }

      i++;
    }

    if (inTable) flushTable();
    return result;
  }, [content]);

  return <div style={style}>{elements}</div>;
}
