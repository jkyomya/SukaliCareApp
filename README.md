# Sukali Care

Sukali Care is a mobile companion app for people living with diabetes who use a continuous glucose monitor (CGM). It pairs with the device over Bluetooth, stores readings on the phone, shows trends and sends alerts for low and high glucose.

Built with .NET MAUI (.NET 8). The current target is Android.

## Features

- Sign in with email or Google (Firebase Authentication)
- Pair a CGM device over Bluetooth and read glucose values in the background
- Dashboard and readings history with charts
- Colour-coded readings and alerts for low and high glucose
- Units in mmol/L or mg/dL
- **Learn**: offline health information on diabetes self-management, weight and activity, and nutrition for southwestern Uganda (`Resources/Raw/health_content.json`)
- **Community**: support group directory and moderated group chat (Cloud Firestore)
- Online health posts (from the backend, opened from the Learn tab)

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

## Health information

All articles live in one file: `Resources/Raw/health_content.json`. Edit the text there and rebuild. No code changes are needed. The content is a draft for clinical review. Before release, check it against the Uganda Clinical Guidelines 2023 (sections 1.1.6, 8.1.3 and 19.1.3).

## Community setup (Firebase)

The community needs a Firebase project. Sign-in uses the same project.

1. Go to [console.firebase.google.com](https://console.firebase.google.com) and sign in. Click **Create a project**, name it (e.g. *Sukali Care*) and finish. Analytics is optional.
2. **Sign-in:** Build > Authentication > Get started > Email/Password > Enable > Save.
3. **Database:** Build > Firestore Database > Create database. Choose a location close to Uganda (e.g. `africa-south1`) and **production mode**.
4. **Rules:** in Firestore, open the **Rules** tab. Replace everything with the contents of `firestore.rules` from this repository. Click **Publish**.
5. **App keys:** Project settings (gear icon) > General > Your apps > Web (`</>`). Register an app. Copy `apiKey`, `authDomain` and `projectId` into `Models/FirebaseConfig.cs`.
6. **Add a support group:** Firestore > Start collection > Collection ID `groups` > Auto-ID. Add these fields (all type *string*): `name`, `description`, `kind` (`in-person`, `online` or `both`), `facility`, `district`, `meetingDay`, `meetingTime`, `contactName`, `contactPhone`.
7. **Make a moderator:** create an account in the app first. Then in Authentication > Users, copy that person's *User UID*. In Firestore, start a collection `moderators` and add a document whose **Document ID is that UID** (add any field, e.g. `role` = `moderator`).

Moderators can hide and show posts from inside the app. Reported posts appear in the `reports` collection in Firestore.

**Data protection.** Group messages are health information. Before inviting patients, agree a privacy notice and consent wording in line with the Uganda Data Protection and Privacy Act, 2019. If the app is part of a study, include the community in your ethics approval.

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
| `Resources/Raw/health_content.json` | Health information articles |
| `firestore.rules` | Community database security rules |
| `Platforms/Android/` | Android Bluetooth and permissions |
