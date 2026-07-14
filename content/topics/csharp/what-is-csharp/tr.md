# C# Nedir?

## Summary

C#, yönetilen bir runtime üzerinde çalışan, statik tipli bir dildir. Bu cümle, yönetilen bir runtime
olmadan neler olduğunu görmeden hiçbir işe yaramaz. O yüzden bu konu bir hatayla başlıyor — C#'ın
yazılmasını imkânsız kıldığı bir hatayla — ve oradan geriye, o imkânsızlığı sağlayan mekanizmaya
yürüyor.

## Learning Objectives

- C#'ın ortadan kaldırdığı bellek hatasını yazabilmek ve patladığında neye mal olduğunu söyleyebilmek.
- Programınız çalıştığında aslında neyin eline teslim edildiğini, ve bunun neden CPU olmadığını açıklamak.
- `Managed Code`'un size ne kazandırdığını ve karşılığında neyi aldığını söylemek.
- Bu takasın sizin için ne zaman yanlış olduğuna karar verebilmek.

## Why This Topic Matters

Sonradan gelen bütün soruların cevabı aynı yerde: bir `.dll` neden Linux'ta da çalışıyor, belleği neden
kimse serbest bırakmıyor, tip hatası neden programı değil derlemeyi durduruyor. Bunu bir kere oturtun,
on iki başka konu şaşırtıcı olmaktan çıkar.

## Definition

C#, `Intermediate Language`'e derlenen ve `Common Language Runtime` tarafından çalıştırılan genel amaçlı
bir dildir. Runtime, program çalışırken belleği yönetir ve `Type Safety`'yi zorlar.

## Why It Exists

Koddan başlayalım. C ile:

```c
char* name = malloc(64);
strcpy(name, "Ada");

free(name);                 // işimiz bitti

printf("%s\n", name);       // ...bir satır geç kaldık
```

Bu ne basar?

Çoğu gün: `Ada`. Bellek serbest bırakıldı ama henüz kimse üstüne yazmadı, baytlar hâlâ orada duruyor.
Program çalışır. Testler geçer. Yayına çıkar.

Bazı günler: çöp. Bazı günler ise **allocator'ın bir sonraki çağırana verdiği şey** — ki bir web
sunucusunda bu, başka birinin oturum token'ıdır.

Ve çökmez. Asıl üzerinde durulması gereken yer burası. Çöken hata, bulduğunuz hatadır. Bu hata bir yıl
boyunca kusursuz çalışır, sonra bir gün başka bir kullanıcının verisini log'a basar.

## Problem It Solves

Yukarıdaki hatanın bir adı var — *use-after-free* — ve bir ailesi: çift serbest bırakma, sarkan
pointer, buffer taşması, sızıntı. Hepsi birlikte, yazılım tarihinde en çok istismar edilmiş hata
sınıfını oluşturuyor.

Hepsinin kaynağı tek bir ayrıcalık: **belleğin ne zaman bırakılacağına siz karar veriyorsunuz.**

C# bu ayrıcalığı elinizden alıyor. `free` diye bir şey yok. Erken çağıracağınız bir şey yok, iki kez
çağıracağınız bir şey yok, unutacağınız bir şey yok. Yukarıdaki kod **yazılamıyor.**

Ve bu, konunun asıl sorusunu doğuruyor:

> Belleği hiç serbest bırakmıyorsanız, kim bırakıyor?

## Core Mental Model

Birinin izliyor olması gerek. Çağırdığınız bir kütüphane değil — programınızın *altında* duran, hangi
nesnelere hâlâ erişilebildiğini bilen bir şey.

O şey `Common Language Runtime`. Kodunuz CPU üzerinde çalışmıyor. CLR üzerinde çalışıyor; CPU üzerinde
çalışan CLR:

```text
   C# kaynak kodu
        │  derleyici
        ▼
   Intermediate Language        ← bir .dll'in içinde gerçekte bulunan şey
        │  Common Language Runtime  (Just-In-Time)
        ▼
   makine kodu                  ← mümkün olan en son anda üretilir
```

Derleyici makine kodu **üretmiyor**. `Intermediate Language` üretiyor — runtime'ın anladığı,
CPU'dan bağımsız komutlar.

Bundan iki şey doğrudan çıkıyor.

**Bellek yönetilebilir**, çünkü runtime kodunuzla makine arasında duruyor. Elinizde hangi referanslar
var biliyor; dolayısıyla artık neye erişemediğinizi de biliyor. Erişilemeyeni geri alabiliyor.

**Aynı `.dll` hem x64 sunucuda hem ARM dizüstünde çalışıyor**, çünkü makine kodu program başlayana kadar
ortada yoktu. `Just-In-Time` derleyicisi onu, gerçekten bulduğu CPU için üretti.

.NET hakkındaki "bu nasıl çalışabiliyor" sorularının neredeyse hepsinin cevabı bu son cümle.

## Core Concepts

Kelimelerin artık konacak bir yeri var.

**Managed Code** — CPU üzerinde doğrudan değil, CLR'ın gözetiminde çalışan kod. Bir miktar denetimden
vazgeçiyorsunuz; karşılığında artık yazamayacağınız koca bir hata sınıfını geri alıyorsunuz.

**Garbage Collector** — hiçbir şeyin erişemediği belleği geri kazanan runtime bileşeni. Yukarıdaki
`free`'nin sorduğu soruyu cevaplıyor. Ama **sizin cevaplayacağınızdan daha geç** cevaplıyor, ve bedel bu.

**Type Safety** — bir değer, yalnızca gerçekte ne ise o şekilde kullanılabilir. Derleyici denetler,
runtime bir daha denetler. `int x = "hello";` çalışırken patlamaz. **Derlenmez.**

**Intermediate Language** — her `.dll`'in içindeki, CPU'dan bağımsız komut kümesi. Ne kaynak kod, ne
makine kodu. JIT'in yediği şey.

## Basic Example

```csharp
var name = "Ada";

Console.WriteLine(name);

// Buraya `name`'i serbest bırakan bir satır ekleyemezsiniz.
// "Eklememelisiniz" değil. EKLEYEMEZSİNİZ. Öyle bir API yok.
```

Metot bitince `name` erişilemez hale gelir. Garbage Collector onu bir süre sonra geri kazanır — ne zaman
olduğu size söylenmez, ve umursamanız da beklenmez.

## Real-World Scenario

Dakikada on bin istek alan bir API. Her istek bellek ayırıyor: bir request nesnesi, ayrıştırılmış JSON,
bir yanıt.

C'de bunların her biri, unutmamanız ve iki kez yapmamanız gereken bir `free` demek — dakikada on bin kez,
beş kişinin elinden geçmiş bir kodda.

C#'ta ayırıyorsunuz ve düşünmeyi bırakıyorsunuz. GC tam da bu şekle göre optimize edilmiş, çünkü nesnelerin
çoğu genç ölüyor: biten isteğe ait olanlar neredeyse bedavaya toplanıyor.

Asıl düşündüğünüz şey duraklama oluyor. Bir toplama, thread'lerinizi durduruyor. Çoğu servis için bu
mikrosaniyelerdir ve kimse fark etmez. Bir alım-satım sistemi için etmeyebilir — ve takasın slogan olmaktan
çıkıp mühendislik kararına dönüştüğü an tam olarak burasıdır.

## Best Practices

- Derleyicinin işinizi almasına izin verin: nullable reference type'lar, `readonly`, `sealed`. Her biri
  çalışma zamanındaki bir olasılığı derleme hatasına çevirir.
- Standart kütüphanede zaten ne olduğunu, onu yazmadan önce öğrenin. Büyük bölümü orada.
- Ölçülmüş bir sebebiniz olmadan Garbage Collector ile güreşmeyin.

## Common Mistakes

**`GC.Collect()` çağırmak.** Neredeyse her zaman yanlış. Tam bir toplamayı, genellikle runtime'ın
seçeceğinden daha kötü bir anda zorlar — ve asıl problemi gizler. O problem de neredeyse her zaman,
birinin unuttuğu bir referansın nesneyi hayatta tutmasıdır.

**`Managed Code`'u "bellek problemi olmaz" diye anlamak.** Belleği **bozamazsınız**. Ama gayet iyi
**sızdırabilirsiniz**. Hiç aboneliği kaldırılmayan bir event handler, bağlı olduğu bütün nesne grafiğini
sonsuza kadar hayatta tutar; ve Garbage Collector her baytını sadakatle korur — çünkü onun durduğu yerden
bakınca, siz o nesneye hâlâ erişebiliyorsunuz.

**`var`'ı "dinamik" sanmak.** Tip derleme zamanında sabitlenir. `var` yalnızca şu demek: "sağ tarafta zaten
söyledin, bana iki kez söyletme."

## Trade-Offs

| Kazandığınız | Vazgeçtiğiniz |
|---|---|
| use-after-free asla olmaz | Belleğin ne zaman bırakılacağı |
| Tip hataları çalışmadan yakalanır | Dinamik dilin bir miktar özgürlüğü |
| Tek binary, çok CPU | Biraz başlangıç süresi (JIT) |
| Kutuda geniş bir kütüphane | Dağıtılacak büyük bir runtime |

Birinci satır bir tercih değil. Artık **yazamayacağınız** bir güvenlik açığı sınıfı, ve dilin var olma
sebebi. Diğer üçü zevk meselesi.

:::warning
GC "bellek problemi olmaz" demek değil. "Bellek **bozulmaz**" demek. Sızıntı hâlâ sizin.
:::

## Further Reading

- CLR yürütme modeli üzerine .NET runtime dokümantasyonu.
- "Yaklaşık olarak" cevabının yetmediği sorular için C# dil şartnamesi.
