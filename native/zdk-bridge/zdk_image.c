/* ZDK image decoder — real implementation over stb_image.
 *
 * Microsoft.Xna.Zune's ZuneImage type P/Invokes these functions to decode
 * PNG/JPEG/BMP assets into straight RGBA bytes for Texture2D.SetData.
 */
#include <stdint.h>
#include <stddef.h>
#include <stdlib.h>
#include <string.h>

#define STB_IMAGE_IMPLEMENTATION
#define STBI_ONLY_PNG
#define STBI_ONLY_JPEG
#define STBI_ONLY_BMP
#include "vendor/stb_image.h"

typedef struct zune_image
{
    uint32_t width;
    uint32_t height;
    unsigned char* bits;
} zune_image;

uint32_t ZDKImage_CreateImageFromBuffer(uint8_t* buf, uint32_t cbBuf, void** phImage)
{
    if (phImage != NULL)
    {
        *phImage = NULL;
    }

    if (buf == NULL || cbBuf == 0 || phImage == NULL)
    {
        return 1;
    }

    int width = 0;
    int height = 0;
    int channels = 0;
    unsigned char* bits = stbi_load_from_memory(buf, (int)cbBuf, &width, &height, &channels, 4);
    if (bits == NULL || width <= 0 || height <= 0)
    {
        return 1;
    }

    zune_image* image = (zune_image*)malloc(sizeof(zune_image));
    if (image == NULL)
    {
        stbi_image_free(bits);
        return 1;
    }

    image->width = (uint32_t)width;
    image->height = (uint32_t)height;
    image->bits = bits;
    *phImage = image;
    return 0;
}

uint32_t ZDKImage_CreateImageFromFile(const uint16_t* filename, void** phImage)
{
    if (phImage != NULL)
    {
        *phImage = NULL;
    }

    if (filename == NULL || phImage == NULL)
    {
        return 1;
    }

    char path[4096];
    size_t out = 0;
    for (size_t i = 0; filename[i] != 0; i++)
    {
        uint32_t cp = filename[i];
        if (cp >= 0xD800 && cp <= 0xDBFF && filename[i + 1] >= 0xDC00 && filename[i + 1] <= 0xDFFF)
        {
            cp = 0x10000 + ((cp - 0xD800) << 10) + (filename[++i] - 0xDC00);
        }

        if (cp < 0x80)
        {
            if (out + 1 >= sizeof(path)) return 1;
            path[out++] = (char)cp;
        }
        else if (cp < 0x800)
        {
            if (out + 2 >= sizeof(path)) return 1;
            path[out++] = (char)(0xC0 | (cp >> 6));
            path[out++] = (char)(0x80 | (cp & 0x3F));
        }
        else if (cp < 0x10000)
        {
            if (out + 3 >= sizeof(path)) return 1;
            path[out++] = (char)(0xE0 | (cp >> 12));
            path[out++] = (char)(0x80 | ((cp >> 6) & 0x3F));
            path[out++] = (char)(0x80 | (cp & 0x3F));
        }
        else
        {
            if (out + 4 >= sizeof(path)) return 1;
            path[out++] = (char)(0xF0 | (cp >> 18));
            path[out++] = (char)(0x80 | ((cp >> 12) & 0x3F));
            path[out++] = (char)(0x80 | ((cp >> 6) & 0x3F));
            path[out++] = (char)(0x80 | (cp & 0x3F));
        }
    }

    path[out] = 0;

    int width = 0;
    int height = 0;
    int channels = 0;
    unsigned char* bits = stbi_load(path, &width, &height, &channels, 4);
    if (bits == NULL || width <= 0 || height <= 0)
    {
        return 1;
    }

    zune_image* image = (zune_image*)malloc(sizeof(zune_image));
    if (image == NULL)
    {
        stbi_image_free(bits);
        return 1;
    }

    image->width = (uint32_t)width;
    image->height = (uint32_t)height;
    image->bits = bits;
    *phImage = image;
    return 0;
}

uint32_t ZDKImage_GetImageSize(void* hImage, uint32_t* cxSize, uint32_t* cySize)
{
    if (hImage == NULL)
    {
        return 1;
    }

    const zune_image* image = (const zune_image*)hImage;
    if (cxSize != NULL) *cxSize = image->width;
    if (cySize != NULL) *cySize = image->height;
    return 0;
}

uint32_t ZDKImage_GetImageData(void* hImage, uint8_t* buf, uint32_t cbBuf)
{
    if (hImage == NULL || buf == NULL)
    {
        return 1;
    }

    const zune_image* image = (const zune_image*)hImage;
    size_t needed = (size_t)image->width * (size_t)image->height * 4u;
    if (cbBuf < needed)
    {
        return 1;
    }

    memcpy(buf, image->bits, needed);
    return 0;
}

uint32_t ZDKImage_ReleaseImage(void* hImage)
{
    if (hImage == NULL)
    {
        return 1;
    }

    zune_image* image = (zune_image*)hImage;
    stbi_image_free(image->bits);
    free(image);
    return 0;
}
