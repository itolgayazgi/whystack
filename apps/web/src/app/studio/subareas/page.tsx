'use client';

import { ApiError, authoringApi, type EditableSubArea, NetworkError } from '@whystack/api-client';
import { type FormEvent, useCallback, useEffect, useState } from 'react';
import { useSession } from '@/lib/session';
import styles from '../studio.module.css';

/** A key is lowercase letters, digits and hyphens — the same shape the server enforces. */
const KEY_PATTERN = /^[a-z0-9-]+$/;

/** A display name → a key guess. The editor can override it, but a sensible default removes a step. */
function toKey(name: string): string {
  return name
    .toLocaleLowerCase('tr')
    .replaceAll('ı', 'i')
    .replaceAll('ş', 's')
    .replaceAll('ğ', 'g')
    .replaceAll('ü', 'u')
    .replaceAll('ö', 'o')
    .replaceAll('ç', 'c')
    .replace(/[^a-z0-9]+/g, '-')
    .replace(/^-+|-+$/g, '');
}

/**
 * The theme vocabulary — managed here, exactly like the terminology dictionary (ADR-0023).
 *
 * A theme is a thread a topic deepens across levels: async is "usage" at Junior, "deadlocks" at Mid,
 * "ValueTask" at Senior. Curated rather than free-typed, because the roadmap slice that will consume it
 * ("async from Junior to Expert") groups on a stable key, and free text would split that thread in silence.
 *
 * A theme in use CANNOT be deleted: the server refuses with a 409 naming the count, because deleting it
 * would silently untag its topics. The editor retags them first.
 */
export default function SubAreasPage() {
  const { client, status: session } = useSession();

  const [subAreas, setSubAreas] = useState<EditableSubArea[]>([]);
  const [load, setLoad] = useState<'loading' | 'ready' | 'unreachable' | 'failed'>('loading');
  const [failure, setFailure] = useState<string>();
  const [busy, setBusy] = useState(false);

  const [name, setName] = useState('');
  const [key, setKey] = useState('');
  const [keyEdited, setKeyEdited] = useState(false);

  const fetchSubAreas = useCallback(async () => {
    setLoad('loading');

    try {
      const { data } = await authoringApi.subAreas(client);
      setSubAreas(data);
      setLoad('ready');
    } catch (error) {
      setLoad(error instanceof NetworkError ? 'unreachable' : 'failed');
    }
  }, [client]);

  useEffect(() => {
    if (session === 'signed-in') void fetchSubAreas();
  }, [session, fetchSubAreas]);

  async function add(event: FormEvent) {
    event.preventDefault();

    const finalKey = keyEdited ? key.trim() : toKey(name);

    if (busy || !name.trim() || !finalKey) return;

    if (!KEY_PATTERN.test(finalKey)) {
      setFailure('Anahtar yalnızca küçük harf, rakam ve tire içerebilir — örneğin "async".');
      return;
    }

    setBusy(true);
    setFailure(undefined);

    try {
      await authoringApi.saveSubArea(client, { key: finalKey, name: name.trim() });
      setName('');
      setKey('');
      setKeyEdited(false);
      await fetchSubAreas();
    } catch (error) {
      setFailure(
        error instanceof NetworkError
          ? 'Sunucuya ulaşılamıyor. Tema EKLENMEDİ.'
          : error instanceof ApiError
            ? error.message
            : 'Tema kaydedilemedi.',
      );
    } finally {
      setBusy(false);
    }
  }

  async function remove(subArea: EditableSubArea) {
    setBusy(true);
    setFailure(undefined);

    try {
      await authoringApi.deleteSubArea(client, subArea.id);
      await fetchSubAreas();
    } catch (error) {
      // The 409 is the whole point: a theme in use is not deleted, and the message says how many topics
      // stand in the way. Anything else is reported plainly.
      setFailure(
        error instanceof ApiError && error.status === 409
          ? error.message
          : error instanceof NetworkError
            ? 'Sunucuya ulaşılamıyor. Tema silinmedi.'
            : 'Tema silinemedi.',
      );
    } finally {
      setBusy(false);
    }
  }

  return (
    <>
      <header className={styles.header}>
        <div>
          <h1 className={styles.title}>Temalar</h1>
          <p className={styles.subtitle}>
            Bir tema, konunun seviyeler boyunca ilerleyen ipliğidir — async, bellek yönetimi. Seviye ve
            kategoriden bağımsız; bir konu bir temaya ait olur ya da hiçbirine.
          </p>
        </div>
      </header>

      {failure ? (
        <div className={styles.notice} role="alert">
          {failure}
        </div>
      ) : null}

      <form className={styles.panel} onSubmit={add}>
        <h2 className={styles.panelTitle}>Tema ekle</h2>

        <div className={styles.grid}>
          <label className={styles.field}>
            <span className={styles.label}>Ad</span>
            <input
              className={styles.input}
              value={name}
              onChange={(event) => {
                setName(event.target.value);
                if (!keyEdited) setKey(toKey(event.target.value));
              }}
              placeholder="Async / Await"
            />
          </label>

          <label className={styles.field}>
            <span className={styles.label}>Anahtar</span>
            <input
              className={styles.input}
              value={keyEdited ? key : toKey(name)}
              onChange={(event) => {
                setKeyEdited(true);
                setKey(event.target.value);
              }}
              placeholder="async"
            />
            {/* The key never changes after creation — tagged topics and roadmap slices resolve through it,
                exactly as a topic's stable key does. Renaming it would orphan them. */}
            <span className={styles.hint}>Bir kez belirlenir, sonra değişmez. Küçük harf, rakam, tire.</span>
          </label>
        </div>

        <div className={styles.actions} style={{ marginTop: 'var(--space-3)' }}>
          <button type="submit" className={styles.primary} disabled={busy || !name.trim()}>
            Ekle
          </button>
        </div>
      </form>

      {load === 'loading' ? <p className={styles.subtitle}>Yükleniyor…</p> : null}

      {load === 'unreachable' ? (
        <div className={styles.notice} role="alert">
          Sunucuya ulaşılamıyor.{' '}
          <button type="button" className={styles.ghost} onClick={() => void fetchSubAreas()}>
            Tekrar dene
          </button>
        </div>
      ) : null}

      {load === 'ready' && subAreas.length === 0 ? (
        <div className={styles.empty}>
          <p>Henüz tema yok.</p>
          <p>İlk temayı ekle — sonra bir konu açarken künyeden seçebilirsin.</p>
        </div>
      ) : null}

      {load === 'ready' && subAreas.length > 0 ? (
        <table className={styles.table}>
          <thead>
            <tr>
              <th>Tema</th>
              <th>Anahtar</th>
              <th>Kullanım</th>
              <th />
            </tr>
          </thead>
          <tbody>
            {subAreas.map((subArea) => (
              <tr key={subArea.id}>
                <td>
                  <span className={styles.rowTitle}>{subArea.name}</span>
                </td>
                <td>
                  <code className={styles.rowKey}>{subArea.key}</code>
                </td>
                <td>
                  {subArea.topicCount > 0 ? (
                    `${subArea.topicCount} konu`
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
                    aria-disabled={subArea.topicCount > 0}
                    title={
                      subArea.topicCount > 0
                        ? `${subArea.topicCount} konu bu temayı kullanıyor. Önce onları başka bir temaya taşı.`
                        : undefined
                    }
                    onClick={() => {
                      if (subArea.topicCount > 0) {
                        setFailure(
                          `"${subArea.name}" ${subArea.topicCount} konu tarafından kullanılıyor. ` +
                            'Silinirse o konular sessizce temasız kalırdı — önce onları taşı.',
                        );
                        return;
                      }
                      void remove(subArea);
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
