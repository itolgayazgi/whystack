'use client';

import {
  ApiError,
  authoringApi,
  type EditableScope,
  type LineOption,
  NetworkError,
} from '@whystack/api-client';
import { type FormEvent, useCallback, useEffect, useState } from 'react';
import { useSession } from '@/lib/session';
import styles from '../studio.module.css';

/** A key is lowercase letters, digits and hyphens — the same shape the server enforces. */
const KEY_PATTERN = /^[a-z0-9-]+$/;

/*
  There is deliberately NO name → key derivation here, and there used to be.

  It lowercased the Turkish name and knocked the diacritics off it: "Dilin Temelleri" became
  `dilin-temelleri`. Keys are English (`07` § Slug and Key Language), so that function could only ever
  produce the wrong answer — and it produced it INTO the box, pre-filled and looking considered, which is the
  worst way to be wrong. An author accepts a filled field; they think about an empty one.

  An English key cannot be derived from a Turkish name. The author types it.
*/

/**
 * The scope vocabulary — managed here, exactly like the terminology dictionary.
 *
 * A scope (Kapsam) is a neighbourhood of 3-10 stops ON ONE LINE: "EF Core" on B3, "Dilin Temelleri" on B1.
 * Curated rather than free-typed, because the roadmap slice that consumes it groups on a stable key, and free
 * text would split a neighbourhood in silence.
 *
 * <b>A scope belongs to exactly one line, and this page used to pretend otherwise.</b> It was written under
 * ADR-0023, when the axis was a "tema" that floated across the whole corpus. ADR-0027 found that Kapsam and
 * theme were one axis, and put it on the line — because B1's "Eşzamanlılık" (threads and locks) is not B3's
 * "Transaction & Eşzamanlılık" (isolation levels), and a single global list cannot tell them apart. The page
 * kept the old vocabulary and never sent a line, so the server refused every create with a 422 and nobody
 * could add a scope at all.
 *
 * A scope in use CANNOT be deleted: the server refuses with a 409 naming the count, because deleting it would
 * silently untag its topics. The editor retags them first.
 */
export default function ScopesPage() {
  const { client, status: session } = useSession();

  const [scopes, setScopes] = useState<EditableScope[]>([]);
  const [lines, setLines] = useState<LineOption[]>([]);
  const [load, setLoad] = useState<'loading' | 'ready' | 'unreachable' | 'failed'>('loading');
  const [failure, setFailure] = useState<string>();
  const [busy, setBusy] = useState(false);

  const [name, setName] = useState('');
  const [key, setKey] = useState('');
  const [lineKey, setLineKey] = useState('');

  const fetchScopes = useCallback(async () => {
    setLoad('loading');

    try {
      // The lines come with them: without a line there is nothing to create a scope ON, so this page cannot
      // do its job with only half of this. One await, both or neither.
      const [scopeList, catalog] = await Promise.all([
        authoringApi.scopes(client),
        authoringApi.catalog(client),
      ]);

      setScopes(scopeList.data);
      setLines(catalog.data.lines);
      setLoad('ready');
    } catch (error) {
      setLoad(error instanceof NetworkError ? 'unreachable' : 'failed');
    }
  }, [client]);

  useEffect(() => {
    if (session === 'signed-in') void fetchScopes();
  }, [session, fetchScopes]);

  async function add(event: FormEvent) {
    event.preventDefault();

    const finalKey = key.trim();

    if (busy || !name.trim() || !finalKey || !lineKey) return;

    if (!KEY_PATTERN.test(finalKey)) {
      setFailure('Anahtar yalnızca küçük harf, rakam ve tire içerebilir — örneğin "ef-core".');
      return;
    }

    // Checked HERE as well as on the server, because the server's answer arrives after a round trip and says
    // it in English. The server's refusal is the one that counts; this one is the one the author reads.
    const clash = scopes.find((scope) => scope.lineKey === lineKey && scope.key === finalKey);

    if (clash) {
      setFailure(
        `Bu hatta "${finalKey}" anahtarı zaten var — "${clash.name}". ` +
          'Anahtar hat içinde benzersizdir; başka bir hatta aynı anahtar serbesttir.',
      );
      return;
    }

    setBusy(true);
    setFailure(undefined);

    try {
      await authoringApi.saveSubArea(client, { key: finalKey, name: name.trim(), lineKey });
      setName('');
      setKey('');

      // The LINE is deliberately not cleared. Scopes are added in batches on one line — "Dilin Temelleri",
      // then "OOP", then "Koleksiyonlar", all on B1 — and re-picking it every time is a step per scope.
      await fetchScopes();
    } catch (error) {
      setFailure(
        error instanceof NetworkError
          ? 'Sunucuya ulaşılamıyor. Kapsam EKLENMEDİ.'
          : error instanceof ApiError
            ? error.message
            : 'Kapsam kaydedilemedi.',
      );
    } finally {
      setBusy(false);
    }
  }

  async function remove(scope: EditableScope) {
    setBusy(true);
    setFailure(undefined);

    try {
      await authoringApi.deleteSubArea(client, scope.id);
      await fetchScopes();
    } catch (error) {
      // The 409 is the whole point: a scope in use is not deleted, and the message says how many topics
      // stand in the way. Anything else is reported plainly.
      setFailure(
        error instanceof ApiError && error.status === 409
          ? error.message
          : error instanceof NetworkError
            ? 'Sunucuya ulaşılamıyor. Kapsam silinmedi.'
            : 'Kapsam silinemedi.',
      );
    } finally {
      setBusy(false);
    }
  }

  return (
    <>
      <header className={styles.header}>
        <div>
          <h1 className={styles.title}>Kapsamlar</h1>
          <p className={styles.subtitle}>
            Bir kapsam, <b>bir hattın üzerinde</b> 3-10 durak eden mahalledir — B1'de "Dilin Temelleri", B3'te
            "EF Core". Kapsam hatta aittir: B1'in "Eşzamanlılık"ı ile B3'ün "Transaction & Eşzamanlılık"ı aynı
            şey değildir. Bir durak bir kapsama girer ya da hiçbirine — ikincisi de normaldir.
          </p>
        </div>
      </header>

      {failure ? (
        <div className={styles.notice} role="alert">
          {failure}
        </div>
      ) : null}

      <form className={styles.panel} onSubmit={add}>
        <h2 className={styles.panelTitle}>Kapsam ekle</h2>

        <div className={styles.grid}>
          {/*
            The LINE, first — the field this page did not have.

            It is first rather than last because it is the question that comes first: you decide which route
            you are laying a neighbourhood on, then you name it. It also scopes the uniqueness of everything
            below it, so filling it in last means the key check has nothing to check against.
          */}
          <label className={styles.field}>
            <span className={styles.label}>Hat — kapsamın üzerinde durduğu rota</span>
            <select
              className={styles.select}
              value={lineKey}
              onChange={(event) => setLineKey(event.target.value)}
            >
              {/* No default. Eight lines and a pre-selected first one is how a B1 scope quietly lands on B3 —
                  the author never looked at a field that was already filled. */}
              <option value="">— hat seç —</option>

              {/* Grouped by area: eight lines is a long flat list, and the author is thinking "Backend, veri
                  erişimi" rather than "b3". */}
              {[...new Set(lines.map((line) => line.areaName))].map((areaName) => (
                <optgroup key={areaName} label={areaName}>
                  {lines
                    .filter((line) => line.areaName === areaName)
                    .map((line) => (
                      <option key={line.key} value={line.key}>
                        {line.name}
                      </option>
                    ))}
                </optgroup>
              ))}
            </select>
            <span className={styles.hint}>Kapsam bir hatta aittir ve sonradan taşınmaz.</span>
          </label>

          <label className={styles.field}>
            <span className={styles.label}>Ad — okuyucunun gördüğü</span>
            <input
              className={styles.input}
              value={name}
              onChange={(event) => setName(event.target.value)}
              placeholder="Dilin Temelleri"
            />
            <span className={styles.hint}>Türkçe. Sonradan değiştirilebilir.</span>
          </label>

          <label className={styles.field}>
            <span className={styles.label}>Anahtar — kalıcı kimlik</span>
            <input
              className={styles.input}
              value={key}
              onChange={(event) => setKey(event.target.value)}
              placeholder="language-basics"
            />
            {/* The key never changes after creation — tagged topics and roadmap slices resolve through it,
                exactly as a topic's stable key does. Renaming it would orphan them.

                The rule lives in `07` § Slug and Key Language. It is repeated here because the author decides
                it HERE, and a rule nobody is shown at the moment they break it is a rule that gets broken. */}
            <span className={styles.hint}>
              <b>İngilizce</b> yaz — ad Türkçe kalır, anahtar kimliktir. Küçük harf, rakam, tire.{' '}
              <b>Hat içinde</b> benzersiz; başka bir hatta aynı anahtar serbest. Bir kez belirlenir, sonra
              değişmez.
            </span>
          </label>
        </div>

        <div className={styles.actions} style={{ marginTop: 'var(--space-3)' }}>
          {/* Disabled without a line: the server refuses it anyway, and a button that fails when pressed is a
              worse answer than one that visibly cannot be. */}
          <button type="submit" className={styles.primary} disabled={busy || !name.trim() || !lineKey}>
            Ekle
          </button>
        </div>
      </form>

      {load === 'loading' ? <p className={styles.subtitle}>Yükleniyor…</p> : null}

      {load === 'unreachable' ? (
        <div className={styles.notice} role="alert">
          Sunucuya ulaşılamıyor.{' '}
          <button type="button" className={styles.ghost} onClick={() => void fetchScopes()}>
            Tekrar dene
          </button>
        </div>
      ) : null}

      {load === 'ready' && scopes.length === 0 ? (
        <div className={styles.empty}>
          <p>Henüz kapsam yok.</p>
          <p>İlk kapsamı ekle — sonra bir durak açarken künyeden seçebilirsin.</p>
        </div>
      ) : null}

      {load === 'ready' && scopes.length > 0 ? (
        <table className={styles.table}>
          <thead>
            <tr>
              {/* The LINE is a column, not a detail. The key is unique per line, so two rows reading
                  "Eşzamanlılık" are two different neighbourhoods — without this column they are a duplicate
                  somebody would try to clean up. */}
              <th>Hat</th>
              <th>Kapsam</th>
              <th>Anahtar</th>
              <th>Kullanım</th>
              <th />
            </tr>
          </thead>
          <tbody>
            {scopes.map((scope) => (
              <tr key={scope.id}>
                <td>
                  <span className={styles.hint}>{scope.lineName}</span>
                </td>
                <td>
                  <span className={styles.rowTitle}>{scope.name}</span>
                </td>
                <td>
                  <code className={styles.rowKey}>{scope.key}</code>
                </td>
                <td>
                  {scope.topicCount > 0 ? (
                    `${scope.topicCount} durak`
                  ) : (
                    <span className={styles.hint}>—</span>
                  )}
                </td>
                <td>
                  <button
                    type="button"
                    className={styles.ghost}
                    disabled={busy}
                    // Disabled when in use, and the title says why — the server would refuse it anyway, but a
                    // button that visibly cannot be pressed beats a button that fails when pressed.
                    aria-disabled={scope.topicCount > 0}
                    title={
                      scope.topicCount > 0
                        ? `${scope.topicCount} durak bu kapsamı kullanıyor. Önce onları başka bir kapsama taşı.`
                        : undefined
                    }
                    onClick={() => {
                      if (scope.topicCount > 0) {
                        setFailure(
                          `"${scope.name}" (${scope.lineName}) ${scope.topicCount} durak tarafından ` +
                            'kullanılıyor. Silinirse o duraklar sessizce kapsamsız kalırdı — önce onları taşı.',
                        );
                        return;
                      }
                      void remove(scope);
                    }}
                  >
                    Sil
                  </button>
                </td>
              </tr>
            ))}
          </tbody>
        </table>
      ) : null}
    </>
  );
}
