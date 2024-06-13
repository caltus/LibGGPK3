﻿using System;
using System.IO;
using System.Net.Http;

using LibBundle3;
using LibBundle3.Records;

using SystemExtensions;

namespace LibBundledGGPK3;
/// <summary>
/// <see cref="DriveBundleFactory"/> but downloads the bundle from the patch server if it doesn't exist.
/// </summary>
/// <remarks>
/// Remember to call <see cref="Dispose"/> when done.
/// </remarks>
/// <param name="baseDirectory">Path on drive to save the downloaded bundles</param>
/// <param name="patchCdnUrl">Can get from <see cref="LibGGPK3.PatchClient.GetPatchCdnUrl"/></param>
public class ServerBundleFactory(string baseDirectory, string patchCdnUrl) : DriveBundleFactory(baseDirectory), IDisposable {
	public Uri CdnUrl => http.BaseAddress!;
	protected readonly HttpClient http = new(new SocketsHttpHandler() { UseCookies = false }) {
		BaseAddress = new(patchCdnUrl),
		DefaultVersionPolicy = HttpVersionPolicy.RequestVersionOrHigher
	};

	public override Bundle GetBundle(BundleRecord record) {
		var rp = record.Path;
		var path = BaseDirectory + rp;
		if (File.Exists(path))
			return new(path, record); // base.GetBundle(record)
		return new(Download("Bundles2/" + rp), false, record);
	}

	private FileStream Download(string path) {
		using var res = http.Send(new(HttpMethod.Get, path));
		if (!res.IsSuccessStatusCode)
			ThrowHelper.Throw<HttpRequestException>($"Failed to download file ({res.StatusCode}): {path}");
		var fs = File.Create(path);
		using (var s = res.Content.ReadAsStream())
			s.CopyTo(fs);
		return fs;
	}
	/// <summary>
	/// Downloads the "Bundles2/_.index.bin" file and saves it to the baseDirectory
	/// </summary>
	public void DownloadIndex() {
		Download("Bundles2/_.index.bin").Dispose();
	}

	public virtual void Dispose() {
		GC.SuppressFinalize(this);
		http.Dispose();
	}
}