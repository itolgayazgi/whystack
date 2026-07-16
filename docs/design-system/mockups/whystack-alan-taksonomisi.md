# WhyStack — Alan Taksonomisi v1 (4 Alanın Tamamı)
### Backend / Frontend / Database / DevOps konu dağıtımı

## 0) Nihai hiyerarşi — tek resim, ezberlenecek tek model

```
ALAN (4 sabit)                    Backend · Frontend · Database · DevOps
 │
 ├── EKOSİSTEM (ağ değiştirici)   Alanına göre değişir; hat DEĞİLDİR.
 │                                Ağın tamamını başka dile/motora çevirir.
 │
 └── HAT (alan başına 5-8)        Haritadaki renkli çizgiler. Bölüm/tema.
      │
      └── KAPSAM (hat başına 3-6) Mahalle. 3-10 durak. Rozet buraya bağlı.
           │                      (kod adı: Scope — eski SubArea)
           └── DURAK (konu)       5-25 dk. Arketipi var.
                │
                └── BLOK          hook, concept, code, checkpoint...
```

**Kurallar her alanda aynıdır.** Alan değişince sadece iki şey değişir:
1. Hat listesi (Backend'de B1-B8, Frontend'de F1-F7...)
2. Ekosistem ekseninin anlamı (aşağıdaki tablo)

| Alan | Ekosistem ekseni | Örnek değerler | Not |
|---|---|---|---|
| Backend | Dil/Platform | .NET · Java · Node.js · Python · Go · Rust | Mevcut karar |
| Frontend | Framework | React · Angular · Vue · Svelte | JS/TS ortak temel + framework slice |
| Database | Motor | SQL Server · PostgreSQL · MySQL · MongoDB | Kavram ortak, syntax/araç motora göre |
| DevOps | Bulut | Bulut-bağımsız · Azure · AWS · GCP | Çoğu konu bulut-bağımsız; slice sadece gerektiğinde |

> Onboarding kararınla birebir uyumlu: "Backend→ekosistem, DevOps→cloud, bazı alanlar ekseni atlar." Ekosistem bir HAT değildir — sekmeden seçilir ve tüm ağı değiştirir.

---

## 1) BACKEND — 8 hat, ~154 durak *(mevcut, referans)*
B1 Dil & Runtime · B2 Web API & Framework · B3 Veri Erişimi · B4 Mimari & Tasarım · B5 Mesajlaşma & Dağıtık · B6 Güvenlik & Kimlik · B7 Test & Kalite · B8 Performans & Gözlemlenebilirlik
(Detay: whystack-backend-taksonomi.md — değişmedi.)

---

## 2) FRONTEND — 7 hat, ~110 durak hedefi

| Hat | Kapsam örnekleri | Jr/Mid/Sr/Exp | Toplam |
|---|---|---|---|
| **F1 Dil & Runtime (JS/TS)** | Tip Sistemi & TS, Event Loop, Modüller, Bellek | 7/6/4/3 | 20 |
| **F2 Bileşen & State** | Component modeli, State yönetimi, Render döngüsü, Form | 6/6/4/2 | 18 |
| **F3 Stil & Layout** | CSS temelleri, Layout (flex/grid), Responsive, Design token | 6/5/3/1 | 15 |
| **F4 Veri & Ağ** | HTTP & fetch, Cache & senkronizasyon, Realtime, Hata/yeniden deneme | 4/6/4/2 | 16 |
| **F5 Performans & Web Vitals** | Yükleme (bundle, lazy), Render performansı, Ölçüm (CWV) | 2/5/5/3 | 15 |
| **F6 Güvenlik** | XSS & injection, CORS & CSP, Auth token saklama | 3/4/3/1 | 11 |
| **F7 Test & Kalite** | Unit/component test, E2E, Erişilebilirlik (a11y) | 3/5/4/1 | 13 |
| **Toplam** | | **31/37/27/13** | **~108** |

Örnek "neden" durakları: *Virtual DOM Neden Var?* (F2-Mid) · *CSS Specificity — Kimin Sözü Geçer?* (F3-Jr) · *"use client" Ne Zaman Yanlış?* (F2-Sr) · *Bundle 2MB Oldu — Kim Şişirdi?* (F5, Production Olayı)

---

## 3) DATABASE — 5 hat, ~85 durak hedefi

| Hat | Kapsam örnekleri | Jr/Mid/Sr/Exp | Toplam |
|---|---|---|---|
| **D1 Veri Modelleme** | Normalizasyon, İlişkiler & anahtarlar, Denormalizasyon kararları | 5/5/3/1 | 14 |
| **D2 Sorgu & Optimizasyon** | SQL derinliği, Index stratejileri, Execution plan, İstatistikler | 6/7/5/3 | 21 |
| **D3 Transaction & Eşzamanlılık** | ACID, İzolasyon seviyeleri, Lock & deadlock, MVCC | 3/5/5/2 | 15 |
| **D4 Yönetim & Operasyon** | Backup/restore, Güvenlik & yetki, Bakım (vacuum/reindex) | 4/5/3/1 | 13 |
| **D5 Ölçekleme & Dağıtık Veri** | Replication, Partitioning/sharding, NoSQL kavramları, CAP | 1/4/6/4 | 15 |
| **Toplam** | | **19/26/22/11** | **~78** |

**Backend B3 ile sınır (kritik, kapsam dokümanındaki kuralın genellemesi):**
- **B3 = uygulamanın veriye bakışı** (ORM, EF Core, uygulama tarafında transaction)
- **Database alanı = verinin kendisi** (motor, plan, index iç yapısı, operasyon)
- Aynı kavram iki tarafta farklı derinlikte yaşayabilir; ⇄ aktarma ile bağlanır.
  Örn: B3 "İzolasyon Seviyeleri — uygulamadan" ⇄ D3 "İzolasyon Seviyeleri — motorun içinden"

> Senin PostgreSQL gelişim hedefin de tam buraya oturuyor: D2 hattı, T-SQL bilen birinin PostgreSQL'e geçişi için motor-slice mantığının ilk gerçek testi olur.

---

## 4) DEVOPS — 6 hat, ~90 durak hedefi

| Hat | Kapsam örnekleri | Jr/Mid/Sr/Exp | Toplam |
|---|---|---|---|
| **O1 Linux & Ağ Temelleri** | Shell & süreçler, Dosya sistemi, DNS/TCP/TLS | 6/5/2/1 | 14 |
| **O2 Container & Orkestrasyon** | Docker imaj & katman, Compose, Kubernetes temel, K8s ileri | 4/6/5/3 | 18 |
| **O3 CI/CD & Otomasyon** | Pipeline anatomisi, Build & artifact, Deploy stratejileri (blue-green, canary) | 3/6/4/2 | 15 |
| **O4 Gözlemlenebilirlik** | Log toplama, Metrik & alarm, Distributed tracing | 2/5/5/2 | 14 |
| **O5 Altyapı & IaC** | IaC kavramı (Terraform), Ortam yönetimi, Ağ topolojisi | 1/4/5/2 | 12 |
| **O6 Güvenlik & Erişim** | Secrets yönetimi, IAM & least privilege, Supply chain | 2/4/4/2 | 12 |
| **Toplam** | | **18/30/25/12** | **~85** |

Örnek "neden" durakları: *Container Neden Var? VM'in Yetmediği An* (O2-Jr) · *İmaj 3GB Oldu — Katmanlar Nerede Şişti?* (O2, Production Olayı) · *Canary Deploy — %1'e Güvenmek* (O3-Sr)

---

## 5) Alanlar arası çakışma kuralları (kafa karışıklığının asıl ilacı)

Aynı kelime birden çok alanda geçer — bu hata değil, bakış açısı farkıdır. Karar tablosu:

| Konu kelimesi | Nerede yaşar | Kural |
|---|---|---|
| Transaction | B3 (uygulamadan) + D3 (motordan) | İki durak, ⇄ aktarma |
| Gözlemlenebilirlik | B8 (kodu enstrümante etmek) + O4 (platformu izlemek) | İki durak, ⇄ aktarma |
| Güvenlik | B6 (API/kimlik) + F6 (tarayıcı) + O6 (altyapı) + D4 (veri) | Her alan kendi yüzeyini anlatır |
| CORS | F6 (tarayıcı neden engelliyor) + B2 (sunucu nasıl izin verir) | İki yarım, birbirine aktarma |
| Test | B7 + F7 (alan içi) | Ortak kavramlar concept-layer'da tek yazılır |

**Tek cümlelik kural:** *Konu, hangi alanın GÜNLÜK İŞİNDE çözülüyorsa orada yaşar; diğer alanlar ona aktarma verir.* Tereddütte kalırsan: "Bu konuda nöbette kim uyanır?" — cevap alanı söyler.

## 6) KnowledgeDomain → yeni model eşlemesi (Claude Code migration referansı)

| Eski değer | Yeni karşılık |
|---|---|
| backend | Area=backend |
| database | Area=database |
| language | Area=backend, Line=B1 |
| architecture | Area=backend, Line=B4 |
| security | Area=backend, Line=B6 *(ileride F6/O6/D4'e alan bazlı dağılır)* |
| testing | Area=backend, Line=B7 |

## 7) Büyük resim ve üretim gerçekliği

| Alan | Hat | Durak hedefi | Lansman durumu |
|---|---|---|---|
| Backend | 8 | ~154 | Faz 1-2 üretimde (mevcut plan) |
| Frontend | 7 | ~108 | Harita görünür, tüm duraklar 🔔 "yakında" |
| Database | 5 | ~78 | Harita görünür, 🔔 |
| DevOps | 6 | ~85 | Harita görünür, 🔔 |
| **Toplam** | **26** | **~425** | |

425 sayısı korkutmasın — bu bir hedef ağ haritası, yazılacaklar listesi değil. Lansman kuralı aynı: **tüm ağ 1. günden haritada görünür, içerik Backend'den başlar, diğer alanların sırasına 🔔 talepleri karar verir.** Frontend/Database/DevOps taksonomileri şu an tek iş görür: haritayı çizmek ve talep sinyali toplamak. İçerikleri, Backend Faz 2 bitmeden yazılmaz.
