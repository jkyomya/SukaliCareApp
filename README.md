# Sukali Care

Sukali Care is a mobile companion app for people living with diabetes who use a continuous glucose monitor (CGM). It pairs with the device over Bluetooth, stores readings on the phone, shows trends and sends alerts for low and high glucose.

Built with .NET MAUI (.NET 8). The current target is Android.

## Features

- Sign in with email or Google (Firebase Authentication)
- Pair a CGM device over Bluetooth and read glucose values in the background
- Dashboard and readings history with charts
- Colour-coded readings and alerts for low and high glucose
- Units in mmol/L or mg/dL
- Library of health education resources

## Glucose bands

Readings are banded using the International Consensus on Time in Range (Battelino et al., *Diabetes Care* 2019).

| Band | mmol/L | mg/dL |
|---|---|---|
| Very low | below 3.0 | below 54 |
| Low | 3.0 to 3.8 | 54 to 69 |
| In range | 3.9 to 10.0 | 70 to 180 |
| High | 10.1 to 13.9 | 181 to 250 |
| Very high | above 13.9 | above 250 |

The thresholds live in `Converters/ValueToColorConvert.cs`. Review them against local guidance before clinical use.

## Device message format

The app sends `ping` to the paired device and expects a reply like:

```
Glucose_reading_6.5_mmol/L_end
```

Parsing is handled in `Services/GlucoseMessageParser.cs`.

## Getting the Android app (APK)

Every push to the default branch builds an APK with GitHub Actions.

1. Open the **Actions** tab of this repository.
2. Click the latest **Android build** run.
3. Download **SukaliCare-android-apk** under *Artifacts* and unzip it.
4. Copy the `.apk` to an Android phone and open it. Allow installs from unknown sources when asked.

This APK is signed with a development key. It is for testing only, not for the Play Store.

## Building locally

Requirements: .NET 8 SDK, Java 17, Android SDK (API 34). Visual Studio 2022 with the .NET MAUI workload installs all of these.

```
dotnet workload install maui-android
dotnet build -f net8.0-android34.0
```

## Backend

The app calls a REST API for user profiles, readings, devices and the library. The base address is set in `Models/Stores.cs` (`BaseUrl`). It currently points to a temporary ngrok address, so those features only work while that server is running. The backend code is not in this repository.

## Project structure

| Folder | Contents |
|---|---|
| `Views/` | Screens (XAML) |
| `ViewModels/` | Screen logic |
| `Models/` | Data models and settings keys |
| `Services/` | Bluetooth, database, Firebase, API and background job |
| `Converters/` | Unit conversion and reading colours |
| `Platforms/Android/` | Android Bluetooth and permissions |
