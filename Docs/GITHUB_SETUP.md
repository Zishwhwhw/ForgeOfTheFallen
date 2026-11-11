# Publish this as a new GitHub repository

The new GitHub repository is already connected as `origin` (`https://github.com/Zishwhwhw/FrankenArms.git`). The local branch is `main` and currently matches `origin/main`. Use the commands below only if you need to publish a new local commit.

## GitHub repository

If recreating the hosted repository later, choose **New repository**, name it `FrankenArms`, choose private or public, and leave **Add a README**, **.gitignore**, and **license** unchecked.

## Publish local commits from PowerShell

Open PowerShell and run these commands, replacing `<OWNER>` with your GitHub username or organization:

```powershell
cd D:\game\2dRoguelike\FrankenArmsFresh
git add -A
git commit -m "Start FrankenArms from GDD"
git remote add origin https://github.com/<OWNER>/FrankenArms.git
git push -u origin main
```

The local repository is already initialized on branch `main` and has the `origin` remote configured. Do not repeat `git remote add` unless you deliberately replace the hosted repository. If Git says `origin` already exists, use `git remote set-url origin <URL>` only when changing remotes.

If Git asks for your name and email before committing, set them for this repository, then repeat the commit:

```powershell
git config user.name "Your Name"
git config user.email "you@example.com"
```

Do not copy `Library`, `Temp`, `Logs`, or `UserSettings` into Git; `.gitignore` excludes them.

## 3. Open the project

```powershell
unity open D:\game\2dRoguelike\FrankenArmsFresh
```

If Unity asks for a license, sign in through Unity Hub. To enable live CLI control of the open Editor, restore access to the Unity Package Registry and run:

```powershell
unity pipeline install --project-path D:\game\2dRoguelike\FrankenArmsFresh
unity status --project-path D:\game\2dRoguelike\FrankenArmsFresh
```

In this Codex session, proxy variables point to the unavailable `127.0.0.1:9`. In a normal PowerShell session, remove only those placeholder values for that session, then retry direct access:

```powershell
$proxyNames = @('HTTP_PROXY','HTTPS_PROXY','ALL_PROXY','http_proxy','https_proxy','all_proxy','UNITY_PROXY')
foreach ($name in $proxyNames) {
  $value = [Environment]::GetEnvironmentVariable($name)
  if ($value -match '^https?://127\.0\.0\.1:9/?$') { Remove-Item "Env:$name" -ErrorAction SilentlyContinue }
}
unity pipeline install --project-path D:\game\2dRoguelike\FrankenArmsFresh --proxy-disable
```

If your network requires a proxy, use its real reachable address instead of removing it.
