'use client';

import { authoringApi, type EditableTerm, NetworkError } from '@whystack/api-client';
import { type FormEvent, useCallback, useEffect, useState } from 'react';
import { useSession } from '@/lib/session';
import styles from '../studio.module.css';

const split = (value: string) =>
  value
    .split(',')
    .map((item) => item.trim())
    .filter(Boolean);

/**
 * The terminology dictionary — the thing that stops "Connection Pooling" becoming "havuzdakiler".
 *
 * `forbiddenTranslations` is the load-bearing field, and it is not obvious why. A translator almost never
 * DROPS a term: it keeps "Connection Pooling" in the heading, where anyone would look, and then paraphrases
 * it for five paragraphs of body text, where nobody does. Checking that the term is present catches nothing.
 * Naming the paraphrase catches it.
 */
export default function TermsPage() {
  const { client, status: session } = useSession();

  const [terms, setTerms] = useState<EditableTerm[]>([]);
  const [load, setLoad] = useState<'loading' | 'ready' | 'unreachable' | 'failed'>('loading');
  const [failure, setFailure] = useState<string>();
  const [busy, setBusy] = useState(false);

  const [text, setText] = useState('');
  const [aliases, setAliases] = useState('');
  const [forbidden, setForbidden] = useState('');
  const [explanation, setExplanation] = useState('');

  const fetchTerms = useCallback(async () => {
    setLoad('loading');

    try {
      const { data } = await authoringApi.terms(client);
      setTerms(data);
      setLoad('ready');
    } catch (error) {
      setLoad(error instanceof NetworkError ? 'unreachable' : 'failed');
    }
  }, [client]);

  useEffect(() => {
    if (session === 'signed-in') void fetchTerms();
  }, [session, fetchTerms]);

  async function add(event: FormEvent) {
    event.preventDefault();

    if (busy || !text.trim()) return;

    setBusy(true);
    setFailure(undefined);

    try {
      await authoringApi.saveTerm(client, {
        text: text.trim(),
        aliases: split(aliases),
        forbiddenTranslations: split(forbidden),
        explanations: explanation.trim() ? [{ languageCode: 'tr', text: explanation.trim() }] : [],
      });

      setText('');
      setAliases('');
      setForbidden('');
      setExplanation('');
      await fetchTerms();
    } catch (error) {
      setFailure(
        error instanceof NetworkError ? 'Sunucuya ulaşılamıyor. Terim EKLENMEDİ.' : 'Terim kaydedilemedi.',
      );
    } finally {
      setBusy(false);
    }
  }

  async function remove(id: string) {
    setBusy(true);
    setFailure(undefined);

    try {
      await authoringApi.deleteTerm(client, id);
      await fetchTerms();
    } catch {
      setFailure('Terim silinemedi.');
    } finally {
      setBusy(false);
    }
  }

  return (
    <>
      <header className={styles.header}>
        <div>
          <h1 className={styles.title}>Terim sözlüğü</h1>
          <p className={styles.subtitle}>
            Terim korunur; yalnızca AÇIKLAMASI çevrilir. Yasak çeviriler alanı, terimin başlıkta durup metinde
            erimesini yakalar — asıl kaçak orada olur.
          </p>
        </div>
      </header>

      {failure ? (
        <div className={styles.notice} role="alert">
          {failure}
        </div>
      ) : null}

      <form className={styles.panel} onSubmit={add}>
        <h2 className={styles.panelTitle}>Terim ekle</h2>

        <div className={styles.grid}>
          <label className={styles.field}>
            <span className={styles.label}>Terim</span>
            <input
              className={styles.input}
              value={text}
              onChange={(event) => setText(event.target.value)}
              placeholder="Connection Pooling"
            />
            <span className={styles.hint}>Çeviriden sağ çıkması gereken tam metin.</span>
          </label>

          <label className={styles.field}>
            <span className={styles.label}>Eş yazımlar</span>
            <input
              className={styles.input}
              value={aliases}
              onChange={(event) => setAliases(event.target.value)}
              placeholder="Connection Pool"
            />
            <span className={styles.hint}>Virgülle ayır.</span>
          </label>

          <label className={styles.field}>
            <span className={styles.label}>Yasak çeviriler</span>
            <input
              className={styles.input}
              value={forbidden}
              onChange={(event) => setForbidden(event.target.value)}
              placeholder="bağlantı havuzlama, havuzdakiler"
            />
            <span className={styles.hint}>Metinde geçerse doğrulama patlar.</span>
          </label>

          <label className={styles.field}>
            <span className={styles.label}>Türkçe açıklama</span>
            <input
              className={styles.input}
              value={explanation}
              onChange={(event) => setExplanation(event.target.value)}
              placeholder="Açılan bağlantıların kapatılmayıp havuzda tutulması."
            />
          </label>
        </div>

        <div className={styles.actions} style={{ marginTop: 'var(--space-3)' }}>
          <button type="submit" className={styles.primary} disabled={busy || !text.trim()}>
            Ekle
          </button>
        </div>
      </form>

      {load === 'loading' ? <p className={styles.subtitle}>Yükleniyor…</p> : null}

      {load === 'unreachable' ? (
        <div className={styles.notice} role="alert">
          Sunucuya ulaşılamıyor.{' '}
          <button type="button" className={styles.ghost} onClick={() => void fetchTerms()}>
            Tekrar dene
          </button>
        </div>
      ) : null}

      {load === 'ready' && terms.length === 0 ? (
        <div className={styles.empty}>
          <p>Sözlük boş.</p>
          <p>Boş bir sözlük hiçbir çeviriyi durduramaz — ilk terimi ekle.</p>
        </div>
      ) : null}

      {load === 'ready' && terms.length > 0 ? (
        <table className={styles.table}>
          <thead>
            <tr>
              <th>Terim</th>
              <th>Eş yazımlar</th>
              <th>Yasak çeviriler</th>
              <th />
            </tr>
          </thead>
          <tbody>
            {terms.map((term) => (
              <tr key={term.id}>
                <td>
                  <span className={styles.rowTitle}>{term.text}</span>
                  {term.explanations[0] ? (
                    <span className={styles.rowKey}>{term.explanations[0].text}</span>
                  ) : null}
                </td>
                <td>
                  <div className={styles.chips}>
                    {term.aliases.map((alias) => (
                      <span key={alias} className={styles.chip}>
                        {alias}
                      </span>
                    ))}
                  </div>
                </td>
                <td>
                  <div className={styles.chips}>
                    {term.forbiddenTranslations.map((banned) => (
                      <span key={banned} className={`${styles.chip} ${styles.chipMissing}`}>
                        {banned}
                      </span>
                    ))}
                  </div>
                </td>
                <td>
                  <button
                    type="button"
                    className={styles.ghost}
                    disabled={busy}
                    onClick={() => void remove(term.id)}
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
