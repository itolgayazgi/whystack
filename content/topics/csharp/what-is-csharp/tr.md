# C# Nedir?

## Summary

C#, .NET üzerinde çalışan, statik tipli ve nesne yönelimli bir dildir. Siz C# yazarsınız, derleyici onu
Intermediate Language'e çevirir, Common Language Runtime da program çalışırken bunu makine koduna
dönüştürür. Bu üç adımı anlamak, dilin geri kalanında gizemli görünen şeylerin çoğunu açıklar.

## Learning Objectives

- C#'ın neye derlendiğini ve gerçekte neyin çalıştığını açıklamak.
- Common Language Runtime'ın rolünü tarif etmek.
- Managed Code'un neden var olduğunu ve size ne kazandırdığını söylemek.
- C#'ın hangi problemleri ortadan kaldırmak için tasarlandığını — ve hangilerini kaldırmadığını — ayırt
  etmek.

## Why This Topic Matters

Sonradan gelen karışıklıkların neredeyse hepsi — bir `.dll`'in neden Linux'ta da çalıştığı, belleğin
neden elle serbest bırakılmadığı, bir tip hatasının neden program başlamadan yakalandığı — yazdığınız
kod ile onu çalıştıran CPU arasında olan bitene dayanır. Bunu bir kez oturtun, on iki başka konu
şaşırtıcı olmaktan çıkar.

## Definition

C#, Intermediate Language'e derlenen ve Common Language Runtime tarafından çalıştırılan genel amaçlı
bir programlama dilidir. Statik tiplidir: her ifadenin tipi program çalışmadan önce bilinir ve
denetlenir.

## Why It Exists

C#, alternatifleri istenmeyen bir takas dayattığı için ortaya çıktı.

C ve C++ size hız ve denetim verdi, belleğin sorumluluğunu da size yükledi — yani koca bir güvenlik
açığı sınıfını, sonsuza kadar, her satırda önlemek sizin işinizdi. Bu yükü sırtınızdan alan diller ise
performanstan, çoğu zaman da statik tiplerden vazgeçti.

C#, bellek yönetimi yükünü üstlenirken hızdan ve tip denetiminden vazgeçmemek üzere tasarlandı. Anlaşma
budur ve adını koymaya değer, çünkü geri kalan her şey bundan çıkar.

## Problem It Solves

Somut problem *use-after-free ve ailesidir*: sarkan pointer'lar, çift serbest bırakma, sızıntılar,
buffer taşmaları. Bunlar egzotik şeyler değildir. Yazılım tarihinin en çok istismar edilen hata
sınıfıdır.

C# bunları, belleği serbest bırakmanıza hiç izin vermeyerek ortadan kaldırır. Programın artık
erişemediği belleği Garbage Collector geri kazanır. Bunun ne zaman olacağı üzerindeki doğrudan
denetimi kaybedersiniz; karşılığında yanlış olamayacağı garantisini alırsınız.

## Historical Context

C# 2000 yılında, Microsoft tarafından, .NET ile birlikte ortaya çıktı. Java, Garbage Collector'a sahip
yönetilen bir runtime'ın ciddi işler için kullanılabilir olduğunu çoktan göstermişti. C# bu önermeden
yola çıktı ve o zamandan beri fonksiyonel dillerden fikirler devşirdi — pattern matching, record'lar,
yer yer varsayılan olarak değişmezlik — ama bir C ya da Java programcısının ilk gün okuyabileceği bir
dil olarak kaldı.

## Core Mental Model

Zihninizde üç katmanı, bu sırayla tutun:

```text
C# kaynak  ──derleyici──▶  Intermediate Language  ──JIT──▶  makine kodu
   (.cs)                         (.dll)                    (CPU'nun çalıştırdığı)
```

Derleyici makine kodu üretmez. Intermediate Language üretir ve bu CPU'dan bağımsızdır. Aynı `.dll`'in
hem x64 bir sunucuda hem ARM bir dizüstünde çalışmasının sebebi budur: makine kodu, mümkün olan en son
anda, Just-In-Time derleyicisi tarafından, gerçekten bulduğu CPU için üretilir.

.NET hakkındaki "bu nasıl çalışabiliyor" sorularının neredeyse hepsinin cevabı, makine kodunun program
başlayana kadar var olmadığını hatırlamaktır.

## Core Concepts

**Statik tipleme.** Her ifadenin, derleyicinin bildiği bir tipi vardır. `int x = "hello";` çalışma
zamanında patlamaz — derlenmez bile. Hata sınıfı, program var olmadan önce ortadan kalkar.

**Managed Code.** Kodunuz doğrudan CPU üzerinde değil, Common Language Runtime'ın gözetiminde çalışır.
Runtime bellek yönetimini, Type Safety'yi ve exception yönetimini sağlar. Bir miktar denetimden
vazgeçersiniz; karşılığında artık yazamayacağınız koca bir hata sınıfını geri alırsınız.

**Garbage Collection.** Bellek, siz söylediğinizde değil, erişilemez hale geldiğinde geri kazanılır.
C++'tan en büyük fark budur; hem C#'ın güvenliğinin hem de zaman zaman yaşanan gecikme sürprizlerinin
kaynağıdır.

**Geniş bir standart kütüphane.** Collection'lar, HTTP, JSON, kriptografi, asenkronluk — kutunun
içinde. C#'ta üretken olmanın büyük bölümü, zaten var olanı öğrenmektir.

## Basic Example

```csharp
// Tip derleme zamanında denetlenir. Bellek çalışma zamanında yönetilir.
// Bu iki cümlenin ikisi de C için doğru değildir.
var greeting = "Hello";
var length = greeting.Length;   // int — ve derleyici bunu bilir

Console.WriteLine($"{greeting} has {length} characters.");
```

`greeting` bir `string`'dir — derleyici bunu çıkarsar, tahmin etmez. `var` dinamik tipleme değildir;
tip derleme zamanında sabitlenir ve sonradan değişemez. Ve burada hiçbir şey string'i serbest bırakmaz:
metot bitince erişilemez hale gelir, gerisini Garbage Collector daha sonra halleder.

## Real-World Scenario

Bir API dakikada on bin istek alıyor. Her biri request nesneleri ayırıyor, JSON ayrıştırıyor, yanıt
oluşturuyor.

C'de bu ayırmaların her biri, unutmamanız ve iki kez yapmamanız gereken bir `free()` demektir. C#'ta
ayırırsınız ve düşünmeyi bırakırsınız. Kısa ömürlü nesneleri Garbage Collector ucuza geri kazanır,
çünkü tam da bu şekle göre optimize edilmiştir: nesnelerin çoğu genç ölür.

Asıl düşündüğünüz şey duraklamalardır. Bir toplama, thread'lerinizi durdurur. Çoğu servis için bu
mikrosaniyelerdir ve kimse fark etmez. Bir alım-satım sistemi için olmayabilir — takasın bir slogan
değil, gerçek bir mühendislik kararı haline geldiği yer tam da burasıdır.

## Best Practices

- Tip sağ taraftan bariz değilse, `var` yerine açık tipi tercih edin. `var` gürültüyü azaltmak içindir,
  bilgiyi saklamak için değil.
- Derleyicinin size yardım etmesine izin verin. Nullable reference type'lar, `readonly`, `sealed` —
  her biri çalışma zamanındaki bir olasılığı derleme zamanındaki bir hataya dönüştürür.
- Standart kütüphanede zaten ne olduğunu, onu yazmadan önce öğrenin. Büyük bölümü orada.
- Ölçülmüş bir sebebiniz olmadan Garbage Collector ile güreşmeyin.

## Common Mistakes

**`var`'ı "dinamik" sanmak.** Değildir. Tip derleme zamanında sabitlenir; `var` yalnızca "sağ tarafta
zaten söyledin, iki kez söyletme" demektir.

**`GC.Collect()` çağırmak.** Neredeyse her zaman yanlıştır. Tam bir toplamayı, genellikle runtime'ın
seçeceğinden daha kötü bir anda zorlar ve asıl problemi gizler — ki o problem genellikle, birinin
unuttuğu bir referansın nesneyi hayatta tutmasıdır.

**Managed Code'u "bellek problemi olmaz" diye anlamak.** Belleği bozamazsınız. Ama gayet iyi
sızdırabilirsiniz — hiç aboneliği kaldırılmayan bir event handler, bağlı olduğu tüm nesne grafiğini
sonsuza kadar hayatta tutar ve Garbage Collector her baytını sadakatle korur.

## Trade-Offs

| Kazandığınız | Vazgeçtiğiniz |
|---|---|
| use-after-free yok, çift serbest bırakma yok, buffer taşması yok | Belleğin ne zaman bırakılacağı üzerindeki belirlenimci denetim |
| Tip hataları program çalışmadan yakalanır | Dinamik bir dilin özgürlüğünün bir kısmı |
| Aynı binary birçok CPU'da çalışır | Just-In-Time derlemesi için bir miktar başlangıç süresi |
| Kutunun içinde geniş bir kütüphane | Kodunuzun yanında dağıtılacak büyük bir runtime |

En çok önem taşıyan satır birincisidir. Diğer satırlar birer tercihtir; o satır ise artık
yazamayacağınız bir güvenlik açığı sınıfıdır — ve dilin var olma sebebidir.

## Further Reading

- Common Language Runtime yürütme modeli üzerine .NET runtime dokümantasyonu.
- "Yaklaşık olarak" cevabının yetmediği sorular için C# dil şartnamesi.
