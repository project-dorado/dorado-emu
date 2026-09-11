/* System / font / media / cloud ZDK stubs.
 *
 * These families are not part of the headless framework path (fonts and audio
 * flow through the XNA content pipeline), so they return "unavailable" and
 * leave output buffers zeroed. The signatures mirror the C# externs in the
 * Zune XNA extension: LPWStr marshals as UTF-16, default strings as UTF-8.
 */
#include <stdint.h>
#include <stddef.h>
#include <string.h>

/* ---- ZDKSystem: input/system services --------------------------------- */

uint32_t ZDKSystem_SignalUserActivity(void) { return 0; }

uint32_t ZDKSystem_ShowKeyboard(const uint16_t* labelText, const uint16_t* commitButtonText, uint32_t flags)
{
    (void)labelText;
    (void)commitButtonText;
    (void)flags;
    return 0;
}

uint32_t ZDKSystem_GetKeyboardState(void) { return 0; }

uint32_t ZDKSystem_SetKeyboardSuggestions(uint16_t* suggestions, int32_t cchSuggestions, uint32_t numSuggestions)
{
    (void)suggestions;
    (void)cchSuggestions;
    (void)numSuggestions;
    return 0;
}

uint32_t ZDKSystem_GetKeyboardBufferText(uint16_t* buf, uint32_t cchBuf, uint32_t* cchBufNeeded)
{
    if (buf != NULL && cchBuf > 0) buf[0] = 0;
    if (cchBufNeeded != NULL) *cchBufNeeded = 0;
    return 0;
}

uint32_t ZDKSystem_SetKeyboardBufferText(const uint16_t* buf)
{
    (void)buf;
    return 0;
}

uint32_t ZDKSystem_CloseKeyboard(void) { return 0; }

uint32_t ZDKSystem_SetOrientation(uint32_t orientation)
{
    (void)orientation;
    return 0;
}

uint32_t ZDKSystem_GetUnlockChallenge(uint8_t* pbbuf, uint32_t cbbuf)
{
    (void)pbbuf;
    (void)cbbuf;
    return 1;
}

uint32_t ZDKSystem_SetUnlockResponse(uint8_t* pbbuf, uint32_t cbbuf)
{
    (void)pbbuf;
    (void)cbbuf;
    return 1;
}

uint32_t ZDKSystem_GetDeviceId(uint8_t* deviceId)
{
    if (deviceId != NULL) memset(deviceId, 0, 16);
    return 0;
}

/* ---- ZDKCloud: offline placeholder ------------------------------------ */

uint32_t ZDKCloud_Connect(void) { return 0; }
void ZDKCloud_Disconnect(void) {}
uint32_t ZDKCloud_ShowConnectivityWizard(void) { return 0; }

uint32_t ZDKCloud_IsConnected(uint32_t* pfConnected)
{
    if (pfConnected != NULL) *pfConnected = 1;
    return 0;
}

uint32_t ZDKCloud_GetConnectionState(uint32_t* pConnectionState)
{
    if (pConnectionState != NULL) *pConnectionState = 1;
    return 0;
}

uint32_t ZDKCloud_CreateJob(uint32_t flags, uint32_t* hCloudJob)
{
    (void)flags;
    if (hCloudJob != NULL) *hCloudJob = 0;
    return 1;
}

uint32_t ZDKCloud_ScheduleJob(uint32_t hParentCloudJob, uint32_t hCloudJob)
{
    (void)hParentCloudJob;
    (void)hCloudJob;
    return 1;
}

uint32_t ZDKCloud_CreateSemaphore(uint32_t hCloudObj, void** hSemaphore)
{
    (void)hCloudObj;
    if (hSemaphore != NULL) *hSemaphore = NULL;
    return 1;
}

uint32_t ZDKCloud_WaitSemaphore(void* hSemaphore, uint32_t timeout)
{
    (void)hSemaphore;
    (void)timeout;
    return 1;
}

uint32_t ZDKCloud_CloseSemaphore(void* hSemaphore)
{
    (void)hSemaphore;
    return 1;
}

uint32_t ZDKCloud_CreateWebRequestTask(
    uint32_t hCloudJob, void* info, uint32_t flags, uint32_t priority,
    uint8_t* requestBuf, uint32_t cbRequestBuf, uint32_t* hCloudTask)
{
    (void)hCloudJob; (void)info; (void)flags; (void)priority;
    (void)requestBuf; (void)cbRequestBuf;
    if (hCloudTask != NULL) *hCloudTask = 0;
    return 1;
}

uint32_t ZDKCloud_CreateWebDownloadTask(
    uint32_t hCloudJob, void* info, uint32_t flags, uint32_t priority, uint32_t* hCloudTask)
{
    (void)hCloudJob; (void)info; (void)flags; (void)priority;
    if (hCloudTask != NULL) *hCloudTask = 0;
    return 1;
}

uint32_t ZDKCloud_GetTaskResults(uint32_t hCloudTask, uint8_t* resultBuf, uint32_t cchResultBuf, uint32_t* cchResultBufNeeded)
{
    (void)hCloudTask; (void)resultBuf; (void)cchResultBuf;
    if (cchResultBufNeeded != NULL) *cchResultBufNeeded = 0;
    return 1;
}

uint32_t ZDKCloud_GetTaskExecutionState(uint32_t hCloudObj, uint32_t* state, uint32_t* hr)
{
    (void)hCloudObj;
    if (state != NULL) *state = 0;
    if (hr != NULL) *hr = 0;
    return 1;
}

uint32_t ZDKCloud_CancelObjectAsync(uint32_t hCloudObj)
{
    (void)hCloudObj;
    return 1;
}

uint32_t ZDKCloud_CloseObject(uint32_t hCloudObj)
{
    (void)hCloudObj;
    return 1;
}

/* ---- ZDKInput: no queued input messages in headless mode --------------- */

uint32_t ZDKInput_GetNextInputMessage(void* msg)
{
    if (msg != NULL) memset(msg, 0, 32);
    return 1;
}

/* ---- ZDKMedia: analysis queue is inert --------------------------------- */

uint32_t ZDKMedia_Queue_StartSongAnalysis(uint32_t hSong, uint32_t msecSampleInterval, uint32_t resultsMode)
{
    (void)hSong; (void)msecSampleInterval; (void)resultsMode;
    return 0;
}

uint32_t ZDKMedia_Queue_GetSongAnalysisData(void* buf, uint32_t cbBuf, uint32_t* cbFilled)
{
    (void)buf; (void)cbBuf;
    if (cbFilled != NULL) *cbFilled = 0;
    return 0;
}

uint32_t ZDKMedia_Queue_StopSongAnalysis(void) { return 0; }

uint32_t ZDKMedia_Queue_Lock(int32_t fLock)
{
    (void)fLock;
    return 0;
}

uint32_t ZDKMedia_Queue_SetPlayPosition(uint32_t msecPlayPosition)
{
    (void)msecPlayPosition;
    return 0;
}

uint32_t ZDKMedia_Item_GetMediaId(uint32_t hItem, uint8_t* mediaId)
{
    (void)hItem;
    if (mediaId != NULL) memset(mediaId, 0, 16);
    return 0;
}

uint32_t Media_Item_GetHashCode(uint32_t handle, uint32_t* hashCode)
{
    (void)handle;
    if (hashCode != NULL) *hashCode = 0;
    return 0;
}
