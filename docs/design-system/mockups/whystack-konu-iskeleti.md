# WhyStack — Konu İskeleti (Content Skeleton) v1

## Temel karar: Sabit şablon değil, "arketip + blok" modeli

Her konu birbirine benzemediği için tek bir sabit şablon işlemez. Çözüm iki katmanlı:

1. **Arketip (konu tipi):** Konunun doğasını belirler ve hangi blokların zorunlu olduğunu söyler.
2. **Blok (içerik birimi):** İçeriğin yapı taşı. Her blok tek iş yapar, JSON olarak saklanır, web ve mobil aynı veriden render edilir.

Böylece "async/await" ile "REST vs gRPC" farklı iskelete sahip olabilir ama aynı blok setinden beslenir — editör tarafında da tek içerik modeli yönetirsin.

---

## 1) Arketipler (6 konu tipi)

| Arketip | Cevapladığı soru | Örnek konu |
|---|---|---|
| **Kavram** | "X neden var?" | Dependency Injection neden var? |
| **Mekanizma** | "X perde arkasında nasıl çalışır?" | async/await gerçekte ne yapar? |
| **Karşılaştırma** | "X mi Y mi, hangi durumda?" | REST vs gRPC, SQL vs NoSQL |
| **Production Olayı** | "Sahada ne patladı, neden?" | Thread pool starvation vakası |
| **Pattern / Mimari** | "Bu problemi nasıl organize ederiz?" | CQRS, Outbox Pattern |
| **Atölye** | "Hadi birlikte kuralım" | Middleware yazmak, Docker'a almak |

Her arketipin **zorunlu blok dizilimi** farklıdır (aşağıda). Yeni konu açarken editör önce arketipi seçer; sistem iskeleti otomatik kurar, editör blokları doldurur.

---

## 2) Blok tipleri (12 blok)

| Kod | Blok | Ne yapar |
|---|---|---|
| `hook` | Kanca | Konuyu tek bir "neden" sorusuyla açar. Marka sesi burada yaşar. |
| `story` | Problem hikâyesi | Konunun icat edilme sebebi; kısa senaryo. |
| `concept` | Kavram / zihinsel model | Analoji + net tanım. |
| `code` | Açıklamalı kod | Kod + vurgulu satır + altında tek paragraf açıklama (annotation). |
| `diagram` | Diyagram | Akış/sequence görseli (SVG). |
| `compare` | Karşılaştırma tablosu | 2-3 kolonlu, "ne zaman hangisi" satırıyla biter. |
| `myth` | Yaygın yanılgı | "X sanılır ama Y'dir" kutusu. |
| `checkpoint` | Kontrol sorusu | Çoktan seçmeli veya "kendi cümlenle açıkla". Cevap açıklaması zorunlu. |
| `prod` | Production notu | "Sahada nerede patlar" — log/metrik düzeyinde somutluk. |
| `term` | Terim tanımı | Sözlüğe bağlanan inline terim (state machine, continuation...). |
| `summary` | Özet | 3-5 maddelik "cebinde kalanlar". |
| `next` | Sonraki durak | Hattaki devam noktası + varsa aktarma (⇄) önerisi. |

### Her konuda ZORUNLU 4 blok (arketipten bağımsız)
1. `hook` — konu asla "tanım" ile açılmaz, soruyla açılır ("Nasıl'dan önce, neden" ilkesi)
2. En az 1 `checkpoint` — pasif okumayı kırar
3. `summary` — cebinde kalanlar
4. `next` — kullanıcı asla çıkmaz sokağa varmaz; her durağın devamı vardır

### Arketip → önerilen dizilim
- **Kavram:** hook → story → concept → code → myth → checkpoint → summary → next
- **Mekanizma:** hook → concept → code(2-3 adet) → diagram → myth → checkpoint → prod → summary → next
- **Karşılaştırma:** hook → story → compare → code(x2, yan yana senaryo) → checkpoint → "kararı sen ver" checkpoint → summary → next
- **Production Olayı:** hook(olayın kendisi) → story(timeline) → diagram → concept(kök neden) → prod → checkpoint → summary → next
- **Pattern:** hook → story(patternsiz dünya) → concept → diagram → code → myth(ne zaman KULLANMA) → checkpoint → summary → next
- **Atölye:** hook → adım blokları (code + checkpoint dönüşümlü) → prod → summary → next

---

## 3) Veri modeli (backend tarafı, .NET'e uygun)

```jsonc
{
  "topicId": "dotnet-mid-async-await",
  "title": "async/await Gerçekte Ne Yapar?",
  "area": "backend",              // backend|frontend|database|devops
  "ecosystem": "dotnet",          // dotnet|java|nodejs|python|go|rust|agnostic
  "level": "mid",                 // junior|mid|senior|expert
  "archetype": "mechanism",       // concept|mechanism|comparison|incident|pattern|workshop
  "durationMin": 18,
  "prerequisites": ["dotnet-junior-threads"],
  "transfers": [                  // hatlar arası aktarma (⇄)
    { "topicId": "devops-mid-thread-metrics", "reason": "Thread pool metriklerini izleme" }
  ],
  "terms": ["state-machine", "continuation", "thread-pool", "starvation"],
  "version": 3,                   // içerik versiyonlama
  "blocks": [
    { "type": "hook", "order": 1,
      "data": { "question": "await yazdığında thread beklemiyor...", "promise": "18 dk sonra..." } },
    { "type": "concept", "order": 2,
      "data": { "analogy": "garson/mutfak", "body": "markdown..." } },
    { "type": "code", "order": 3,
      "data": { "file": "OrderService.cs", "lang": "csharp", "source": "...",
                 "highlightLines": [4], "annotation": "Derleyici bu metodu..." } },
    { "type": "checkpoint", "order": 4,
      "data": { "question": "...", "options": ["a...","b...","c..."],
                 "correct": 1, "explanation": "Thread serbest kalır çünkü..." } },
    { "type": "summary", "order": 7, "data": { "items": ["...", "..."] } },
    { "type": "next", "order": 8,
      "data": { "topicId": "dotnet-mid-configureawait", "label": "Bu durağın devamı" } }
  ]
}
```

**Tablo önerisi (SQL Server):**
- `Topics` — künye alanları kolon olarak (filtreleme/harita bunlardan beslenir)
- `TopicBlocks` — `TopicId`, `Order`, `BlockType`, `DataJson (nvarchar(max))` — blok içeriği JSON
- `TopicVersions` — yayınlanan her sürümün snapshot'ı (içerik güncellenince ilerleme bozulmaz)
- Kullanıcı ilerlemesi **blok bazında** tutulur: `UserBlockProgress(UserId, TopicId, BlockOrder, CompletedAt)` → mobildeki segment bar ve "kaldığın yerden devam" doğrudan buradan gelir.

---

## 4) Render kuralları (web vs mobil, aynı JSON'dan)

| Blok | Web | Mobil |
|---|---|---|
| Tüm bloklar | Tek sayfada akış + sol scrollspy haritası | Her blok = 1 segment; üst çubuk blok blok dolar |
| `code` | Geniş, satır vurgulu, kopyala butonu | Kısaltılmış snippet (3-8 satır); "tam kodu aç" ile genişler |
| `diagram` | Inline SVG | Yatay kaydırılabilir veya tam ekran aç |
| `compare` | Tablo | Kart kart kaydırma (swipe) |
| `checkpoint` | Inline, cevap açıklaması altında açılır | Tam genişlik kart; cevaplamadan sonraki blok kilitli kalabilir (opsiyonel) |
| `summary`+`next` | Altın çerçeveli kapanış | Sticky alt bar "sonraki durak"a dönüşür |

**Mobil ilkesi:** Mobilde konu KISALTILMAZ, bloklara BÖLÜNÜR. Aynı derinlik, daha küçük lokmalar.

---

## 5) Editör iş akışı (senin için)

1. Konu aç → arketip seç → sistem zorunlu blok iskeletini kurar
2. Blokları doldur (Claude'a arketip + iskelet verildiğinde taslağı üretebilir; sen teknik doğruluğu onaylarsın)
3. Checkpoint'siz veya hook'suz konu **yayınlanamaz** (validasyon kuralı — FluentValidation'lık iş)
4. Yayınla → `TopicVersions`'a snapshot düşer → harita/hat otomatik güncellenir

## 6) İlk içerik seti önerisi (MVP)

Her arketipten 1'er örnek yazıp iskeleti test et (6 konu), sonra .NET Mid basamağını doldur. Instagram playbook'undaki "Neden Var?" serisi ile `hook` blokları birebir aynı ses — sosyal içerik, platform konularının kancasından türetilebilir (tek üretim, iki kanal).
