﻿using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace LibGGPK3 {
	public static class Extensions {
		public static string ExpandPath(in string path) {
			if (path.StartsWith('~')) {
				if (path.Length == 1) { // ~
					var userProfile = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile, Environment.SpecialFolderOption.None);
					if (!string.IsNullOrEmpty(userProfile))
						return Environment.ExpandEnvironmentVariables(userProfile);
				} else if (path[1] is '/' or '\\') { // ~/...
					var userProfile = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile, Environment.SpecialFolderOption.None);
					if (!string.IsNullOrEmpty(userProfile))
						return Environment.ExpandEnvironmentVariables(userProfile + path[1..]);
				}
				try { // ~username/...
					if (!OperatingSystem.IsWindows()) {
						string bash;
						if (File.Exists("/bin/zsh"))
							bash = "/bin/zsh";
						else if (File.Exists("/bin/var/bash"))
							bash = "/bin/var/bash";
						else if (File.Exists("/bin/bash"))
							bash = "/bin/bash";
						else
							return Environment.ExpandEnvironmentVariables(path);
						using var p = Process.Start(new ProcessStartInfo(bash) {
							CreateNoWindow = true,
							ErrorDialog = true,
							RedirectStandardInput = true,
							RedirectStandardOutput = true,
							WindowStyle = ProcessWindowStyle.Hidden
						});
						p!.StandardInput.WriteLine("echo " + path);
						var tmp = p.StandardOutput.ReadLine();
						p.Kill();
						if (!string.IsNullOrEmpty(tmp))
							return tmp;
					}
				} catch { }
			}
			return Environment.ExpandEnvironmentVariables(path);
		}

		/// <summary>
		/// Get patch server url to download bundle files in ggpk
		/// </summary>
		/// <exception cref="SocketException" />
		[SkipLocalsInit]
		public static string GetPatchServer(bool garena = false) {
			using var tcp = new Socket(SocketType.Stream, ProtocolType.Tcp);
			if (garena)
				tcp.Connect(Dns.GetHostAddresses("login.tw.pathofexile.com"), 12999);
			else
				tcp.Connect(Dns.GetHostAddresses("us.login.pathofexile.com"), 12995);
			Span<byte> b = stackalloc byte[256];
			b[0] = 1;
			b[1] = 4;
			tcp.Send(b[..2]);
			tcp.Receive(b);
			return ((ReadOnlySpan<byte>)b).Slice(35, b[34] * 2).GetUTF16String();
		}

		/// <summary>
		/// Allocate memory for string with specified count of char
		/// </summary>
		public static readonly Func<int, string> FastAllocateString = typeof(string).GetMethod("FastAllocateString", BindingFlags.Static | BindingFlags.NonPublic)?.CreateDelegate<Func<int, string>>() ?? (length => new('\0', length));

		public static unsafe string GetUTF16String(this ReadOnlySpan<byte> buffer) {
			var str = FastAllocateString(buffer.Length / 2);
			fixed (char* p = str)
				buffer.CopyTo(new(p, buffer.Length));
			return str;
		}

		public static unsafe string ReadUTF16String(this Stream stream, int charCount) {
			var str = FastAllocateString(charCount);
			fixed (char* p = str)
				stream.ReadExactly(new(p, charCount * 2));
			return str;
		}

		[SkipLocalsInit]
		public static unsafe T Read<T>(this Stream stream) where T : unmanaged {
			T value;
			stream.ReadExactly(new(&value, sizeof(T)));
			return value;
		}

		public static unsafe void Write<T>(this Stream stream, in T value) where T : unmanaged {
			fixed (T* p = &value)
				stream.Write(new(p, sizeof(T)));
		}
	}
}