Proje Başlığı: SecureChat - Güvenli Mobil Mesajlaşma Uygulaması 

Öğrenci: Ziya Eren Altaş - 21290571 

Ders: iOS ile Mobil Uygulama Geliştirme 

1. Proje Özeti: 

Bu proje, kullanıcıların birebir ve grup halinde güvenli mesajlaşmasını sağlayan, kullanıcı profili oluşturulabilen ve modern arayüze sahip bir mobil uygulamadır. Veriler Firebase yerine kendi geliştirdiğimiz .NET API ve SQLite veritabanında tutulmaktadır.


2. Kullanılan Teknolojiler:

Mobil: Flutter (Dart)

Backend: ASP.NET Core Web API 8.0

Veritabanı: SQLite (Entity Framework Core)

Mimari: REST API, Client-Server


3. Tamamlanan Özellikler:

Kullanıcı Sistemi: Kayıt, Giriş, Profil Düzenleme (Fotoğraf, Nickname).

Mesajlaşma: Anlık mesajlaşma, İletildi/Okundu bilgisi (Gri/Mavi Tik).

Grup Sohbeti: Grup kurma, üye ekleme, grup fotoğrafı değiştirme.

Güvenlik: Mesajların Base64 tabanlı şifrelenerek iletilmesi.


Gelişmiş Özellikler:

Planlı Mesaj Gönderme (Zamanlayıcı).

Mesaj Silme (Benden / Herkesten).

Yıldızlı Mesajlar ve Kategorileme.

Mesaj Sabitleme ve Cevaplama.

4. Veritabanı Yapısı (ER Diyagramı Özeti):

Users: Kullanıcı kimlik ve profil bilgileri.

Groups: Grup adı, açıklaması ve kurucu bilgisi.

GroupMembers: Hangi kullanıcının hangi grupta olduğu.

Messages: Mesaj içeriği, gönderen, alıcı (veya grup ID), zaman damgası ve durum bayrakları.
