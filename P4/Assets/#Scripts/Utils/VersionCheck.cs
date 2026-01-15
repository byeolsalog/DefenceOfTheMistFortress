using UnityEngine;
using System.Net.Http;
using System.Threading.Tasks;

public static class VersionCheck
{
    private static readonly HttpClient _httpClient = new HttpClient();

    public static string LatestVersion { get; private set; }
    public static string Notice { get; private set; }

    public static async Task<EVersionCheckResult> CheckVersionAsync()
    {
        try
        {
            HttpResponseMessage response = await _httpClient.GetAsync($"{Config.WebServerUrl}{WebServerRequest.Version}{WebServerRequest.Android}");
            response.EnsureSuccessStatusCode();

            string json = await response.Content.ReadAsStringAsync();
            VersionResponse data = Newtonsoft.Json.JsonConvert.DeserializeObject<VersionResponse>(json);

            if (data == null || string.IsNullOrEmpty(data.minVersion))
            {
                Debug.LogError("서버에서 버전 정보를 읽을 수 없습니다.");
                return EVersionCheckResult.Failed;
            }

            LatestVersion = data.latestVersion;
            Notice = data.notice;
            string localVersion = Application.version;

            if (IsLowerVersion(localVersion, data.minVersion))
            {
                Debug.LogError("앱 버전이 낮아 강제 업데이트가 필요합니다.");
                return EVersionCheckResult.ForceUpdate;
            }

            if (IsLowerVersion(localVersion, data.latestVersion))
            {
                return data.forceUpdate ? EVersionCheckResult.ForceUpdate : EVersionCheckResult.OptionalUpdate;
            }

            return EVersionCheckResult.UpToDate;
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"버전 체크 중 오류 발생: {ex.Message}");
            return EVersionCheckResult.Failed;
        }
    }

    private static  bool IsLowerVersion(string local, string target)
    {
        var lv = new System.Version(local);
        var tv = new System.Version(target);
        return lv.CompareTo(tv) < 0;
    }

    [System.Serializable]
    private class VersionResponse
    {
        public string latestVersion;
        public string minVersion;
        public bool forceUpdate;
        public string notice;
    }
}