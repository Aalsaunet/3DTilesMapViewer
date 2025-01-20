using System.Text;
using Newtonsoft.Json;
using System.Threading.Tasks;
using System.Net.Http;
using System;
using System.Threading;
using System.IO;
using System.Collections.Generic;

public class BarentsWatchReceiver
{
    public delegate void OnNewBarentsWatchMessageReceivedHandler(dynamic payload);
    public event OnNewBarentsWatchMessageReceivedHandler NewBarentsWatchMessageReceived;
    public delegate void OnDiagnosticMessageHandler(string msg);
    public event OnDiagnosticMessageHandler NewDiagnosticMessage;
    private GeoBounds bounds;
    private readonly HttpClient client;
    private readonly Dictionary<int, bool> isRelevantMMSI;

    public BarentsWatchReceiver() {
        client = new HttpClient();
        isRelevantMMSI = new Dictionary<int, bool>();
    }

    public void SetRelevantMMSIGeoBounds(GeoBounds geoBounds) {
        bounds = geoBounds;
        isRelevantMMSI.Clear();
    }

    public async Task RunGetStreamForAll(string bwToken, DateTime bwTokenExpire, CancellationToken cancellationToken) {             
        try {
            using var request = new HttpRequestMessage(HttpMethod.Get, APIConsts.BW_DATA_URL_COMBINED_FULL);
            request.Headers.Add("Authorization", $"Bearer {bwToken}");
            using var response = await client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead);
            if (!response.IsSuccessStatusCode) 
                return;

            using var reader = new StreamReader(await response.Content.ReadAsStreamAsync());
            
            while (!reader.EndOfStream && DateTime.Now < bwTokenExpire) {
                if (cancellationToken.IsCancellationRequested) {
                    request.Dispose();
                    response.Content.Dispose();
                    response.Dispose();
                    reader.Close();
                    break;
                }

                var line = await reader.ReadLineAsync();
                if (line != null) {
                    var payload = JsonConvert.DeserializeObject<dynamic>(line);
                    if (payload == null)
                        continue;
                    
                    int mmsi = (int)payload.mmsi;
                    if (isRelevantMMSI.ContainsKey(mmsi)){
                        if (isRelevantMMSI[mmsi])
                            NewBarentsWatchMessageReceived.Invoke(payload);      
                    } else if (bounds.Contains((double?)payload.latitude, (double?)payload.longitude)) {
                        isRelevantMMSI.Add(mmsi, true);
                        NewBarentsWatchMessageReceived.Invoke(payload);
                    } else {
                        isRelevantMMSI.Add(mmsi, false);
                    }   
                }
                
                // if (reader.BaseStream.Length % 100 == 0) {
                //     // NewDiagnosticMessage.Invoke($"Barentswatch stream length is now {reader.BaseStream.Length}. Position: {reader.BaseStream.Position}");
                //     NewDiagnosticMessage.Invoke("Hello from the BW stream!");
                // }                            
            }
        } catch (Exception ex) {
            Console.WriteLine($"Exception: {ex.Message}");
        }
    }

    // public async Task RunGetStreamForAll(string bwToken, DateTime bwTokenExpire, CancellationToken cancellationToken, 
    //                                     GeoBounds geoBounds) {         
    //     var request = new HttpRequestMessage(HttpMethod.Get, APIConsts.BW_GET_ALL_URL);
    //     request.Headers.Add("Authorization", $"Bearer {bwToken}");
    //     try {
    //         var client = new HttpClient();
    //         var response = await client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead);

    //         if (!response.IsSuccessStatusCode) 
    //             return;

    //         using var stream = await response.Content.ReadAsStreamAsync();
    //         using var reader = new StreamReader(stream);
            
    //         while (!reader.EndOfStream && DateTime.Now < bwTokenExpire) {
    //             if (cancellationToken.IsCancellationRequested)
    //                 break;

    //             var line = await reader.ReadLineAsync();
    //             var payload = JsonConvert.DeserializeObject<dynamic>(line);
    //             if (payload != null && geoBounds.Contains(payload.latitude, payload.longitude))
    //                     NewBarentsWatchMessageReceived.Invoke(payload);                                
    //         }
    //     } catch (Exception ex) {
    //         Console.WriteLine($"Exception: {ex.Message}");
    //     }
    // }

    // public async Task RunStreamForMMSIs(string bwToken, DateTime bwTokenExpire, CancellationToken cancellationToken) { 
    //     if (bwToken == null || bwTokenExpire == null || cancellationToken == null || targetMMSIs == null) {
    //         return;
    //     }

    //     var requestData = new { mmsi = targetMMSIs, modelType = "Full", Downsample = false };
    //     var json = JsonConvert.SerializeObject(requestData);
    //     var request = new HttpRequestMessage(HttpMethod.Post, APIConsts.BW_DATA_URL);
    //     request.Headers.Add("Authorization", $"Bearer {bwToken}");
    //     request.Content = new StringContent(json, Encoding.UTF8, "application/json");

    //     try {
    //         var client = new HttpClient();
    //         var response = await client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead);

    //         if (!response.IsSuccessStatusCode) 
    //             return;

    //         using var stream = await response.Content.ReadAsStreamAsync();
    //         using var reader = new StreamReader(stream);
            
    //         while (!reader.EndOfStream && DateTime.Now < bwTokenExpire) {
    //             if (cancellationToken.IsCancellationRequested)
    //                 break;

    //             var line = await reader.ReadLineAsync();
    //             var payload = JsonConvert.DeserializeObject<dynamic>(line);
    //             NewBarentsWatchMessageReceived.Invoke(payload);
    //         }
    //     } catch (Exception ex) {
    //         Console.WriteLine($"Exception: {ex.Message}");
    //     }
    // }

    // public async Task RunStreamForGeoArea(string bwToken, DateTime bwTokenExpire, CancellationToken cancellationToken) {
    //     if (bwToken == null || bwTokenExpire == null || cancellationToken == null || bounds == null) {
    //         return;
    //     }

    //     StringBuilder requestBody = new();
    //     requestBody.Append("{ "); 
    //     requestBody.Append("\"geometry\": ");
    //     requestBody.Append("{ "); 
    //     requestBody.Append("\"type\": \"Polygon\", ");
    //     requestBody.Append("\"coordinates\": ");
    //     requestBody.Append("[");
    //     requestBody.Append(SerializeCoordinates(areaCoordinates));
    //     requestBody.Append("]");
    //     requestBody.Append("},");
    //     requestBody.Append("\"modelType\": \"Simple\", ");
    //     requestBody.Append("\"modelFormat\": \"Json\", ");
    //     requestBody.Append("\"downsample\": \"true\"");
    //     requestBody.Append("}");         
        
    //     var request = new HttpRequestMessage(HttpMethod.Post, APIConsts.BW_DATA_URL2);
    //     request.Headers.Add("Authorization", $"Bearer {bwToken}");
    //     request.Content = new StringContent(requestBody.ToString(), Encoding.UTF8, "application/json");

    //     try {
    //         var client = new HttpClient();
    //         var response = await client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead);

    //         if (!response.IsSuccessStatusCode) 
    //             return;

    //         using var stream = await response.Content.ReadAsStreamAsync();
    //         using var reader = new StreamReader(stream);
            
    //         while (!reader.EndOfStream && DateTime.Now < bwTokenExpire) {
    //             if (cancellationToken.IsCancellationRequested)
    //                 break;

    //             var line = await reader.ReadLineAsync();
    //             var payload = JsonConvert.DeserializeObject<dynamic>(line);
    //             NewBarentsWatchMessageReceived.Invoke(payload);
    //         }
    //     } catch (Exception ex) {
    //         Console.WriteLine($"Exception: {ex.Message}");
    //     }
    // }

    // public async Task RunPostStreamForAll(string bwToken, DateTime bwTokenExpire, CancellationToken cancellationToken) {         
    //     if (string.IsNullOrWhiteSpace(bwToken))
    //         return;

    //     var requestData = new { modelType = "Simple", downsample = false };
    //     var json = JsonConvert.SerializeObject(requestData);
    //     var request = new HttpRequestMessage(HttpMethod.Post, APIConsts.BW_POST_ALL_URL);
    //     request.Headers.Add("Authorization", $"Bearer {bwToken}");
    //     request.Content = new StringContent(json, Encoding.UTF8, "application/json");

    //     try {
    //         using HttpClient client = new();
    //         var response = await client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead);

    //         if (!response.IsSuccessStatusCode) {
    //             Console.WriteLine($"Error: {response.StatusCode} - {response.ReasonPhrase}");
    //             return;
    //         }
                
    //         using var stream = await response.Content.ReadAsStreamAsync();
    //         using var reader = new StreamReader(stream);

    //         while (!reader.EndOfStream && DateTime.Now < bwTokenExpire) {
    //             if (cancellationToken.IsCancellationRequested)
    //                 break;

    //             var line = await reader.ReadLineAsync();
    //             if (line.IsNullOrEmpty())
    //                 continue;
                
    //             try {
    //                 var payload = JsonConvert.DeserializeObject<BarentswatchAisPosition>(line);
    //                 if (payload == null || payload.Latitude == null || payload.Longitude == null) 
    //                     continue;
                    
    //                 if (bounds.Contains(payload.Latitude, payload.Longitude))
    //                     NewBarentsWatchMessageReceived.Invoke(payload);

    //             } catch (Exception ex) {
    //                 Console.WriteLine($"{DateTime.Now}: Error: {ex.Message}");
    //             }
    //         }
    //     } catch (Exception ex) {
    //         Console.WriteLine($"Exception: {ex.Message}");
    //     }
    // }

    // public static string SerializeCoordinates(Coordinate[] coordinates) {
    //     StringBuilder sb = new("[");
    //     foreach (var coordinate in coordinates) {
    //         sb.Append(coordinate.PrintLongLat()).Append(",");
    //     }
    //     sb.Length--; // Remove last comma
    //     sb.Append("]");
    //     return sb.ToString();
    // } 
}
