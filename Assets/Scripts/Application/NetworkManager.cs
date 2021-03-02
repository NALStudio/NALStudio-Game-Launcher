using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

namespace NALStudio.GameLauncher.Networking
{
	public class NetworkManager : MonoBehaviour
	{
		public static bool InternetAvailable { get; private set; } = true;
		public delegate void InternetAvailabilityChangeHandler(bool available);
		public static event InternetAvailabilityChangeHandler InternetAvailabilityChange;

		public void Start()
		{
			InvokeRepeating(nameof(UpdateInternet), 1, 10);
		}

		void UpdateInternet()
		{
			StartCoroutine(CheckInternet());
		}

		IEnumerator CheckInternet()
		{
			NativeArray<bool> result = new NativeArray<bool>(1, Allocator.Persistent);
			CheckJob job = new CheckJob
			{
				jobResult = result
			};
			JobHandle handle = job.Schedule();
			yield return new WaitUntil(() => handle.IsCompleted);
			handle.Complete();
			bool available = result[0];
			result.Dispose();
			if (available != InternetAvailable)
			{
				InternetAvailable = available;
				InternetAvailabilityChange?.Invoke(InternetAvailable);
			}
		}

		struct CheckJob : IJob
		{
			public NativeArray<bool> jobResult;

			public void Execute()
			{
				const string NCSI_TEST_URL = "http://www.msftncsi.com/ncsi.txt";
				const string NCSI_TEST_RESULT = "Microsoft NCSI";
				const string NCSI_DNS = "dns.msftncsi.com";
				const string NCSI_DNS_IP_ADDRESS = "131.107.255.255";

				try
				{
					// Check NCSI test link
					var webClient = new WebClient();
					string result = webClient.DownloadString(NCSI_TEST_URL);
					if (result != NCSI_TEST_RESULT)
					{
						jobResult[0] = false;
						return;
					}

					// Check NCSI DNS IP
					var dnsHost = Dns.GetHostEntry(NCSI_DNS);
					if (dnsHost.AddressList.Length < 1 || dnsHost.AddressList[0].ToString() != NCSI_DNS_IP_ADDRESS)
					{
						jobResult[0] = false;
						return;
					}
				}
				catch (Exception ex)
				{
					Debug.LogError(ex);
					jobResult[0] = false;
					return;
				}

				jobResult[0] = true;
				return;
			}
		}
	}
}