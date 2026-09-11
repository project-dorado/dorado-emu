/* ZDK font engine — TrueType rasterization over stb_truetype.
 *
 * Microsoft.Xna.Zune's FontInternal P/Invokes these functions to lay out and
 * rasterize glyphs into the managed font atlas. Output is a straight-alpha
 * white ARGB pixel (0xAARRGGBB); Texture2D.SetData premultiplies on upload.
 *
 * Font files are resolved at runtime from DORADO_FONT_DIR (the external
 * firmware asset corpus), then common system font directories. No font is
 * ever bundled.
 */
#include <dirent.h>
#include <stdint.h>
#include <stddef.h>
#include <stdio.h>
#include <stdlib.h>
#include <string.h>
#include <strings.h>

#define STB_TRUETYPE_IMPLEMENTATION
#include "vendor/stb_truetype.h"

#ifndef DORADO_MAX_FONT_BYTES
#define DORADO_MAX_FONT_BYTES (32u * 1024u * 1024u)
#endif

typedef struct zune_font
{
    unsigned char* data;
    stbtt_fontinfo info;
    float scale;
    int ascent;
    int descent;
    int line_gap;
} zune_font;

static const char* const kFontDirs[] = {
    "/usr/share/fonts/truetype",
    "/usr/share/fonts",
    "/usr/local/share/fonts",
    "/System/Library/Fonts",
    "C:\\Windows\\Fonts",
};

static unsigned char* read_file(const char* path, size_t* out_size)
{
    FILE* file = fopen(path, "rb");
    if (file == NULL)
    {
        return NULL;
    }

    if (fseek(file, 0, SEEK_END) != 0)
    {
        fclose(file);
        return NULL;
    }

    long size = ftell(file);
    if (size <= 0 || (size_t)size > DORADO_MAX_FONT_BYTES)
    {
        fclose(file);
        return NULL;
    }

    rewind(file);
    unsigned char* data = (unsigned char*)malloc((size_t)size);
    if (data == NULL)
    {
        fclose(file);
        return NULL;
    }

    size_t read = fread(data, 1, (size_t)size, file);
    fclose(file);
    if (read != (size_t)size)
    {
        free(data);
        return NULL;
    }

    *out_size = (size_t)size;
    return data;
}

static int try_font_path(const char* path, unsigned char** out_data)
{
    size_t size = 0;
    unsigned char* data = read_file(path, &size);
    if (data == NULL)
    {
        return 0;
    }

    *out_data = data;
    return 1;
}

/* Builds candidate file names for a typeface plus style suffixes. */
static int load_typeface(const char* dir, const char* typeface, uint32_t style, unsigned char** out_data)
{
    char compact[128];
    size_t c = 0;
    for (size_t i = 0; typeface[i] != 0 && c + 1 < sizeof(compact); i++)
    {
        char ch = typeface[i];
        if (ch != ' ' && ch != '-' && ch != '_')
        {
            compact[c++] = (ch >= 'A' && ch <= 'Z') ? (char)(ch - 'A' + 'a') : ch;
        }
    }
    compact[c] = 0;

    const char* suffixes[6];
    int suffix_count = 0;
    int bold = (style & 4u) != 0;
    int italic = (style & 2u) != 0;
    if (bold && italic) { suffixes[suffix_count++] = "_bi"; suffixes[suffix_count++] = "z"; }
    else if (bold) { suffixes[suffix_count++] = "_b"; suffixes[suffix_count++] = "bd"; suffixes[suffix_count++] = "b"; }
    else if (italic) { suffixes[suffix_count++] = "_i"; suffixes[suffix_count++] = "i"; }
    suffixes[suffix_count++] = "";

    char path[1024];
    for (int i = 0; i < suffix_count; i++)
    {
        snprintf(path, sizeof(path), "%s/%s%s.ttf", dir, compact, suffixes[i]);
        if (try_font_path(path, out_data))
        {
            return 1;
        }

        snprintf(path, sizeof(path), "%s/%s%s.TTF", dir, compact, suffixes[i]);
        if (try_font_path(path, out_data))
        {
            return 1;
        }
    }

    /* Case-insensitive directory scan covers corpora whose file names keep
       their original casing (for example ZegoeUI.ttf). */
    DIR* folder = opendir(dir);
    if (folder == NULL)
    {
        return 0;
    }

    char wanted[128];
    struct dirent* entry;
    while ((entry = readdir(folder)) != NULL)
    {
        const char* dot = strrchr(entry->d_name, '.');
        if (dot == NULL || strcasecmp(dot, ".ttf") != 0)
        {
            continue;
        }

        size_t name_len = (size_t)(dot - entry->d_name);
        if (name_len >= sizeof(wanted))
        {
            continue;
        }

        int matched = 0;
        for (int i = 0; i < suffix_count; i++)
        {
            snprintf(wanted, sizeof(wanted), "%s%s", compact, suffixes[i]);
            if (strlen(wanted) == name_len && strncasecmp(entry->d_name, wanted, name_len) == 0)
            {
                matched = 1;
                break;
            }
        }

        if (!matched)
        {
            continue;
        }

        snprintf(path, sizeof(path), "%s/%s", dir, entry->d_name);
        if (try_font_path(path, out_data))
        {
            closedir(folder);
            return 1;
        }
    }

    closedir(folder);
    return 0;
}

static const char* const kFontFallbacks[] = {
    "DejaVuSans.ttf", "NotoSans-Regular.ttf", "FreeSans.ttf", "LiberationSans-Regular.ttf",
    "Arial.ttf", "arial.ttf", "AdwaitaSans-Regular.ttf",
};

/* Recursively looks for a preferred fallback file in a font tree. */
static int scan_font_tree_named(const char* dir, int depth, unsigned char** out_data)
{
    if (depth > 4)
    {
        return 0;
    }

    DIR* folder = opendir(dir);
    if (folder == NULL)
    {
        return 0;
    }

    struct dirent* entry;
    while ((entry = readdir(folder)) != NULL)
    {
        const char* dot = strrchr(entry->d_name, '.');
        if (dot == NULL || (strcasecmp(dot, ".ttf") != 0 && strcasecmp(dot, ".otf") != 0))
        {
            continue;
        }

        for (size_t i = 0; i < sizeof(kFontFallbacks) / sizeof(kFontFallbacks[0]); i++)
        {
            if (strcasecmp(entry->d_name, kFontFallbacks[i]) != 0)
            {
                continue;
            }

            char path[1024];
            snprintf(path, sizeof(path), "%s/%s", dir, entry->d_name);
            if (try_font_path(path, out_data))
            {
                closedir(folder);
                return 1;
            }
        }
    }

    rewinddir(folder);
    while ((entry = readdir(folder)) != NULL)
    {
        if (entry->d_name[0] == '.')
        {
            continue;
        }

        char sub[1024];
        snprintf(sub, sizeof(sub), "%s/%s", dir, entry->d_name);
        DIR* probe = opendir(sub);
        if (probe == NULL)
        {
            continue;
        }

        closedir(probe);
        if (scan_font_tree_named(sub, depth + 1, out_data))
        {
            closedir(folder);
            return 1;
        }
    }

    closedir(folder);
    return 0;
}

/* Recursively returns the first TrueType face (preferring .ttf over .otf). */
static int scan_font_tree_any(const char* dir, int depth, unsigned char** out_data)
{
    if (depth > 4)
    {
        return 0;
    }

    DIR* folder = opendir(dir);
    if (folder == NULL)
    {
        return 0;
    }

    struct dirent* entry;
    while ((entry = readdir(folder)) != NULL)
    {
        const char* dot = strrchr(entry->d_name, '.');
        if (dot == NULL || strcasecmp(dot, ".ttf") != 0)
        {
            continue;
        }

        char path[1024];
        snprintf(path, sizeof(path), "%s/%s", dir, entry->d_name);
        if (try_font_path(path, out_data))
        {
            closedir(folder);
            return 1;
        }
    }

    rewinddir(folder);
    while ((entry = readdir(folder)) != NULL)
    {
        if (entry->d_name[0] == '.')
        {
            continue;
        }

        char sub[1024];
        snprintf(sub, sizeof(sub), "%s/%s", dir, entry->d_name);
        DIR* probe = opendir(sub);
        if (probe == NULL)
        {
            continue;
        }

        closedir(probe);
        if (scan_font_tree_any(sub, depth + 1, out_data))
        {
            closedir(folder);
            return 1;
        }
    }

    closedir(folder);
    return 0;
}

static int load_font_bytes(const char* typeface, uint32_t style, unsigned char** out_data)
{
    const char* font_dir = getenv("DORADO_FONT_DIR");
    if (font_dir != NULL && font_dir[0] != 0)
    {
        if (load_typeface(font_dir, typeface, style, out_data))
        {
            return 1;
        }

        /* A single-file override wins over name matching. */
        char path[1024];
        snprintf(path, sizeof(path), "%s/font.ttf", font_dir);
        if (try_font_path(path, out_data))
        {
            return 1;
        }
    }

    for (size_t i = 0; i < sizeof(kFontDirs) / sizeof(kFontDirs[0]); i++)
    {
        if (load_typeface(kFontDirs[i], typeface, style, out_data))
        {
            return 1;
        }
    }

    /* Many distributions keep each family in its own subdirectory, so a flat
       name probe at the root of the font directory is not enough. */
    for (size_t i = 0; i < sizeof(kFontDirs) / sizeof(kFontDirs[0]); i++)
    {
        if (scan_font_tree_named(kFontDirs[i], 0, out_data))
        {
            return 1;
        }
    }

    if (font_dir != NULL && font_dir[0] != 0 && scan_font_tree_named(font_dir, 0, out_data))
    {
        return 1;
    }

    /* Last resort: any TrueType face on the system, so layout still runs. */
    for (size_t i = 0; i < sizeof(kFontDirs) / sizeof(kFontDirs[0]); i++)
    {
        if (scan_font_tree_any(kFontDirs[i], 0, out_data))
        {
            return 1;
        }
    }

    if (font_dir != NULL && font_dir[0] != 0)
    {
        return scan_font_tree_any(font_dir, 0, out_data);
    }

    return 0;
}

static int debug_enabled(void)
{
    const char* flag = getenv("DORADO_ZDK_DEBUG");
    return flag != NULL && flag[0] == '1';
}

uint32_t ZDKFont_Create(const char* szTypeface, float fPointSize, uint32_t dwStyle, void** phFont)
{
    if (phFont != NULL)
    {
        *phFont = NULL;
    }

    if (szTypeface == NULL || phFont == NULL || fPointSize <= 0.0f)
    {
        if (debug_enabled())
        {
            fprintf(stderr, "[ZDK] ZDKFont_Create invalid args (typeface=%s size=%f)\n",
                    szTypeface != NULL ? szTypeface : "(null)", (double)fPointSize);
        }

        return 1;
    }

    unsigned char* data = NULL;
    if (!load_font_bytes(szTypeface, dwStyle, &data))
    {
        if (debug_enabled())
        {
            fprintf(stderr, "[ZDK] ZDKFont_Create could not resolve '%s' (style=%u, size=%.2f)\n",
                    szTypeface, dwStyle, (double)fPointSize);
        }

        return 1;
    }

    zune_font* font = (zune_font*)calloc(1, sizeof(zune_font));
    if (font == NULL)
    {
        free(data);
        return 1;
    }

    font->data = data;
    int offset = stbtt_GetFontOffsetForIndex(data, 0);
    if (offset < 0 || !stbtt_InitFont(&font->info, data, offset))
    {
        free(data);
        free(font);
        return 1;
    }

    /* FontBase requests point sizes at a 96 DPI output resolution. */
    float pixel_height = fPointSize * (96.0f / 72.0f);
    font->scale = stbtt_ScaleForPixelHeight(&font->info, pixel_height);
    stbtt_GetFontVMetrics(&font->info, &font->ascent, &font->descent, &font->line_gap);
    *phFont = font;

    if (debug_enabled())
    {
        fprintf(stderr, "[ZDK] ZDKFont_Create '%s' size=%.2f style=%u -> ok (pixelHeight=%.1f)\n",
                szTypeface, (double)fPointSize, dwStyle, (double)pixel_height);
    }

    return 0;
}

uint32_t ZDKFont_Destroy(void* hFont)
{
    if (hFont == NULL)
    {
        return 1;
    }

    zune_font* font = (zune_font*)hFont;
    free(font->data);
    free(font);
    return 0;
}

uint32_t ZDKFont_GetFontMetrics(void* hFont, void* pMetrics)
{
    if (hFont == NULL || pMetrics == NULL)
    {
        return 1;
    }

    zune_font* font = (zune_font*)hFont;
    float scale = font->scale;
    float ascent = (float)font->ascent * scale;
    float descent = -(float)font->descent * scale;
    float line_height = (float)(font->ascent - font->descent + font->line_gap) * scale;

    float max_width = 0.0f;
    float max_advance = 0.0f;
    for (int ch = 32; ch < 127; ch++)
    {
        int advance = 0;
        int lsb = 0;
        stbtt_GetCodepointHMetrics(&font->info, ch, &advance, &lsb);
        float scaled = (float)advance * scale;
        if (scaled > max_advance) max_advance = scaled;
    }

    /* Widest glyph box across a representative ASCII range. */
    for (int ch = 32; ch < 127; ch++)
    {
        int x0 = 0, y0 = 0, x1 = 0, y1 = 0;
        stbtt_GetCodepointBitmapBox(&font->info, ch, scale, scale, &x0, &y0, &x1, &y1);
        float width = (float)(x1 - x0);
        if (width > max_width) max_width = width;
    }

    float* metrics = (float*)pMetrics;
    metrics[0] = line_height;
    metrics[1] = ascent;
    metrics[2] = descent;
    metrics[3] = max_width;
    metrics[4] = ascent + descent;
    metrics[5] = max_advance;
    return 0;
}

uint32_t ZDKFont_GetCharMetrics(void* hFont, uint16_t ch, void* pMetrics)
{
    if (hFont == NULL || pMetrics == NULL)
    {
        return 1;
    }

    zune_font* font = (zune_font*)hFont;
    int advance = 0;
    int lsb = 0;
    stbtt_GetCodepointHMetrics(&font->info, ch, &advance, &lsb);

    int x0 = 0, y0 = 0, x1 = 0, y1 = 0;
    stbtt_GetCodepointBitmapBox(&font->info, ch, font->scale, font->scale, &x0, &y0, &x1, &y1);

    float* metrics = (float*)pMetrics;
    metrics[0] = (float)x0;
    metrics[1] = (float)y0;
    metrics[2] = (float)x1;
    metrics[3] = (float)y1;
    metrics[4] = 0.0f;
    metrics[5] = (float)advance * font->scale;
    metrics[6] = 0.0f;
    return 0;
}

uint32_t ZDKFont_DrawCharToBuffer(
    void* hFont, uint16_t ch, uint32_t* pvBuffer, int32_t cbBuffer, int32_t cbPitch,
    int32_t width, int32_t height, int32_t insetX, int32_t insetY)
{
    if (hFont == NULL || pvBuffer == NULL || width <= 0 || height <= 0)
    {
        return 1;
    }

    if (cbBuffer < width * height * 4 || cbPitch < width * 4)
    {
        return 1;
    }

    zune_font* font = (zune_font*)hFont;
    int x0 = 0, y0 = 0, x1 = 0, y1 = 0;
    stbtt_GetCodepointBitmapBox(&font->info, ch, font->scale, font->scale, &x0, &y0, &x1, &y1);

    int glyph_w = x1 - x0;
    int glyph_h = y1 - y0;
    if (glyph_w <= 0 || glyph_h <= 0)
    {
        return 0;
    }

    unsigned char* bitmap = (unsigned char*)malloc((size_t)glyph_w * (size_t)glyph_h);
    if (bitmap == NULL)
    {
        return 1;
    }

    stbtt_MakeCodepointBitmap(&font->info, bitmap, glyph_w, glyph_h, glyph_w, font->scale, font->scale, ch);

    for (int y = 0; y < glyph_h; y++)
    {
        int py = insetY + y;
        if (py < 0 || py >= height)
        {
            continue;
        }

        for (int x = 0; x < glyph_w; x++)
        {
            unsigned char alpha = bitmap[(y * glyph_w) + x];
            if (alpha == 0)
            {
                continue;
            }

            int px = insetX + x;
            if (px < 0 || px >= width)
            {
                continue;
            }

            pvBuffer[(py * width) + px] = ((uint32_t)alpha << 24) | 0x00FFFFFFu;
        }
    }

    free(bitmap);
    return 0;
}
