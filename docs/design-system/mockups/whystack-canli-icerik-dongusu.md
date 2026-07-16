# WhyStack — Canlı İçerik & Geri Çağırma Döngüsü v1
### "Mahalleye yeni bina eklendi" mekanizması

## Fikrin özü

Bir kapsam (mahalle) statik değildir. Kullanıcı 10 durağı bitirip kapsamı tamamlar; sonra 11. durak eklendiğinde ona haber verilir: *"EF Core mahallene yeni bir durak eklendi — incelemek ister misin?"*

Bu tek hamle üç şeyi birden çözer:
1. **Retention:** "Bitirdim, sildim" davranışını kırar. Tamamlamış kullanıcıyı geri çağırmanın en meşru sebebi yeni içeriktir.
2. **İçerik tazeliği algısı:** Platform yaşıyor hissi verir (.NET 10 çıktı → yeni duraklar → herkes haberdar).
3. **Üretim baskısını kaldırır:** 154 konuyu lansmanda bitirme zorunluluğu yoktur; her yeni durak bir geri çağırma fırsatına dönüşür. İçerik takvimi = bildirim takvimi.

---

## Tamamlanma durumu nasıl davranır? (kritik kural)

Yeni durak eklendiğinde kullanıcının emeği ASLA geri alınmaz:

| Öğe | 11. durak eklenince |
|---|---|
| Kapsam rozeti (EF Core ✓) | **Korunur.** Rozetin üstüne küçük altın "+" işareti gelir |
| Kapsam ilerlemesi | "Tamamlandı" yerine **"10/11 · 1 yeni"** gösterilir |
| Basamak yüzdesi (%64 gibi) | **Etkilenmez.** Yeni duraklar seviye barajına sonradan dahil edilmez* |
| Haritadaki durak | Yeni durak **nabız (pulse) animasyonlu altın halka** ile görünür |

\* Aksi hâlde "dün %100'düm, bugün %91 oldum" hissi cezalandırma gibi çalışır. Yeni içerik ödül kapısıdır, borç değil. (Teknikte: seviye eşiği, kullanıcının seviyeye girdiği andaki snapshot durak setine göre hesaplanır — `TopicVersions` mantığının seviye düzeyine uygulanması.)

---

## Bildirim tasarımı (marka sesiyle)

### Push (kontekstüel, izinliyse)
> **EF Core mahallene yeni durak açıldı** 🏗️
> "Compiled Query — Sorguyu Önceden Pişirmek" · Mid · 15 dk
> Rozetin duruyor, sadece mahalle büyüdü.

### Uygulama içi (push izni yoksa — guest-first stratejiyle uyumlu)
- **Bugün sekmesi:** "Günün Menüsü"ne "🆕 Mahallene yeni durak" kartı düşer.
- **Harita:** yeni durakta pulse halkası + hat başında "1 yeni" rozeti.
- **Profil:** kapsam rozetinde "+" işareti.

### E-posta (haftalık bülten, opsiyonel)
"Hat Bülteni" — o hafta açılan tüm duraklar tek mailde. Metro ağı duyurusu gibi: *"Bu hafta ağda 3 yeni durak açıldı."*

---

## Anti-spam kuralları (güveni koruyan taraf)

1. **Kişiselleştirme şartı:** Bildirim yalnızca kullanıcının TAMAMLADIĞI veya AKTİF olduğu kapsamlara yeni durak eklendiğinde gider. Hiç girmediği Rust hattındaki yenilik ona push olmaz (bültende görünebilir).
2. **Frekans tavanı:** Push için kullanıcı başına haftada en fazla 1 "yeni durak" bildirimi. Aynı hafta 3 durak açıldıysa tek bildirimde birleştirilir: "Mahallene 3 yeni durak açıldı."
3. **Sessiz saat + zaman dilimi:** Kullanıcının aktif olduğu saat aralığına gönderilir (basit heuristik: son oturum saatleri).
4. **Tek dokunuş kapatma:** "Bu kapsam için bildirim alma" seçeneği bildirim içinde.

---

## İki farklı olay tipi (karıştırmamak önemli)

| Olay | Ne oldu | Kullanıcıya etkisi |
|---|---|---|
| **Yeni durak** | Kapsama yeni konu eklendi | Bildirim + "10/11" + pulse. Yukarıdaki akış |
| **Durak güncellendi** | Mevcut konunun içeriği revize edildi (ör. .NET 10 değişikliği) | Push YOK. Durak kartında sessiz "Güncellendi · v4" etiketi; tamamlamış kullanıcı açarsa "Bu durak sen bitirdikten sonra güncellendi — değişen bloklar işaretli" şeridi |

Güncellenen blokların işaretlenmesi `TopicVersions` diff'inden gelir — blok bazlı ilerleme tuttuğumuz için "sadece değişen 2 bloğu oku, 5 dk" diyebiliriz. Bu, tam okumadan çok daha yüksek dönüş alır.

---

## Veri modeli ekleri

```
ContentEvents(Id, Type[NewTopic|TopicUpdated], TopicId, ScopeId, LineId, PublishedAt)
UserScopeStatus(UserId, ScopeId, CompletedAt, LastSeenTopicCount)
NotificationLog(UserId, EventId, Channel, SentAt, OpenedAt, CompletedTopicAt)
```

- Bildirim hedefleme sorgusu: `UserScopeStatus.CompletedAt != null AND ScopeId = event.ScopeId`
- `LastSeenTopicCount` → "1 yeni" sayacını hesaplar.
- `NotificationLog.CompletedTopicAt` → döngünün ana metriği.

## Ana metrik

**Geri çağırma dönüş oranı:** bildirimi alan tamamlamış kullanıcıların, 7 gün içinde yeni durağı bitirme yüzdesi. Bu oran, "saves ve DM" gibi platformun kuzey yıldızı metriklerinin uygulama içi karşılığıdır — içerik takviminin neye öncelik vereceğine bu veriyle karar verilir.

## Sosyal medya bağlantısı

Her "yeni durak" olayı otomatik olarak Instagram içerik kuyruğuna aday düşer: durak hook'u → "Neden Var?" carousel taslağı. Platform bildirimi içeride, sosyal post dışarıda aynı olaydan beslenir — tek olay, iki kanal.
