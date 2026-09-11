# `.zcp` decryption — mechanism and key import

**Status:** the encryption mechanism is fully reverse-engineered from the
device modules; Dorado ships **no keys** and does not break DRM. This document
records what the device does, what Dorado implements, and what a user needs to
supply for content they own.

## How a `.zcp` is protected

1. The NX header (`docs/container-formats.md`) marks encrypted packages with
   `0x44 = 2`, `0x48 = 1`, `0x4C = 0x100`.
2. The ZCSTFS region — hash table and all data blocks, from the hash offset at
   `0xD8` to the end of the last stored block — is **AES-ECB** encrypted
   (`0x100` = 256-bit key). ECB was confirmed empirically: a constant
   plaintext block (padding) produces one ciphertext block repeated tens of
   thousands of times per package, and the repeated value is unique per
   package.
3. The per-package AES key is **not** stored in the clear. The 256 bytes at
   header `0xF0`–`0x1F0` are an **RSA-2048 ciphertext** (256 bytes).
4. On device, `zcstfs.dll` (`FSD_MountDisk`) reads that blob and calls the
   key-vault driver (`KEY1:`, IOCTL `0x220054` / `0x220020`). `keyvault.dll`
   acquires a crypto provider, imports an **RSA-2048 private key** from a
   provisioned **keypack**, and `CryptDecrypt`s the blob; the 32-byte result is
   the AES content key used to decrypt the volume.

### The keypack

`keyvault.dll` loads an 8 KiB keypack whose first `0x20` bytes are the SHA-256
of the remaining `0x1FE0` bytes (`FUN_c0b363e4`). The non-production path is
`\Windows\ZuneKeyPack_NonProd_2009.dat`; retail devices are provisioned at
manufacture (`VProvision` / `FuseBurn` registry state under
`Drivers\BuiltIn\KeyVault`). The keypack embeds the RSA private key blob
(`PRIVATEKEYBLOB`, `0x494` bytes for RSA-2048, `RSA2` magic).

We scanned the available corpora for a keypack — the baseline firmware cab
(`NK.bin`, `EXT.bin`, `Recovery.bin`, `ZBoot.bin`), the PC Zune 4.8 installer
payloads, the WMDRM components, and the OpenZDK deploy kit — using the exact
SHA-256 self-hash property, and found none. Retail keypacks live in
device-provisioned storage, not in the update image, and no public leak or
community tool for `.zcp` content keys was found (searched XDA, ZuneWiki /
Zune-Research, the Zune community servers, archive.org, and GitHub). The
practical conclusion: **offline decryption requires the unwrapped key from a
device (or its keypack); the wrapping key is not derivable from the `.zcp`.**

## What Dorado supports

- **Plaintext volumes** (format version 1, e.g. the XNA runtime volume) parse
  and extract with no keys at all.
- **Encrypted volumes** parse and decrypt when a provider supplies the
  **unwrapped AES content key** through `IDrmKeyProvider`. The CLI accepts a
  JSON sidecar:

  ```json
  { "keys": [ { "key": "000102030405060708090a0b0c0d0e0f101112131415161718191a1b1c1d1e1f" } ] }
  ```

  Key a specific package with `"guid"`, or omit `guid` for a default key.
  ```bash
  dorado unpack "Alarm Clock.zcp" out/ --key-file keys.json
  ```

- The `top_hash_table_hash` field in the volume descriptor is a ready-made
  verification oracle: after decryption, `SHA1` of the first `0x4000` bytes of
  the hash region must equal that value. A wrong key fails the block SHA-1
  checks immediately (each hash entry is the SHA-1 of its data block).

## Getting a key (owner path)

Extraction from a physical, user-owned Zune HD is outside this repository.
The community tooling (`CUB3D/zuneslayer`, [Project Lyra](https://github.com/project-lyra-zune/project-lyra))
provides kernel read/write on firmware v4.5 and can read the provisioned
`\Windows\ZuneKeyPack*.dat`, or dump the plaintext `\gametitle` mount while an
app is running. Keys and decrypted Microsoft content must stay private: never
commit, bundle, or redistribute them.

## Implementation map

| Concern | Where |
|---|---|
| Container/volume parsing | `src/Dorado.Containers/Zcp/ZcstfsReader.cs` |
| AES-ECB region decryption | `ZcstfsReader.DecryptEcb` |
| Key seam | `src/Dorado.Containers/Drm/IDrmKeyProvider.cs` |
| JSON key sidecar | `src/Dorado.Containers/Drm/JsonDrmKeyProvider.cs` |
| CLI plumbing | `src/Dorado.Cli/Program.cs` (`--key-file`) |
