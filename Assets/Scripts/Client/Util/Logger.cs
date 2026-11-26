using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Security.Permissions;
using UnityEngine;
using System.Runtime.InteropServices;

public class Logger : IDisposable
{
	public static readonly string LogPath = "Logs";
	public static readonly Logger MainLogger = new Logger("main");

	private readonly string name;
	private readonly string filepath;
	private readonly string directory;
	private readonly BufferedStream textStream;
	private readonly FileStream writer;
	private readonly Queue<string> writeQueue;
	private bool isWriting = false;
	private bool isFileWriting = false;
	private bool errored = false;

	private Logger(string name)//, int readTimeoutMillis, int writeTimeoutMillis) 
	{
		name = name.ToLower();
		foreach (char c in name) {
			if (!char.IsLetterOrDigit(c) && c != '_')
			{
				throw new ArgumentException("Logger " + name + " cannot be created. Logger names must only contain alphanumeric characters and underscores.");
			}
		}

		this.name = name;
		this.filepath = Path.Combine(Application.persistentDataPath, LogPath, $"{name}.txt");
		this.directory = Path.Combine(Application.persistentDataPath, LogPath);

		if (!Directory.Exists(this.directory))
		{
			Directory.CreateDirectory(this.filepath);
		}

		Debug.Log("Log file: " + this.filepath);

		try
		{
			this.textStream = new BufferedStream(new MemoryStream());
			this.writer = new FileStream(this.filepath, FileMode.OpenOrCreate, FileAccess.Write);
			this.writeQueue = new Queue<string>();

			if (this.textStream.CanTimeout)
			{
				//this.textStream.WriteTimeout = writeTimeoutMillis;
				//this.textStream.ReadTimeout = readTimeoutMillis;
				Debug.Log("Text Stream for Logger " + this.name + " can Timeout, Read timeout: " + this.textStream.ReadTimeout + ", Write Timeout: " + this.textStream.WriteTimeout);
			}
		}
		catch (Exception e)
		{
			Debug.LogError("Error opening " + e.Source + ": " + e.Message + " - Logger will not read or write on this instance.");
			errored = true;
		}
	}
	
	public void AppendAsync(string text, bool appendSysTime = true)
	{
		if (errored)
		{
			return;
		}

		if (appendSysTime)
		{
			text = "[" + DateTime.Now.ToString("G") + "]  :  " + text;
		}

		if (!isWriting && !isFileWriting) 
		{
			this.BeginWrite(text);
			isWriting = true;
		}
		else
		{
			this.writeQueue.Enqueue(text);
		}
	}

	private void BeginWrite(string text)
	{
		if (errored)
		{
			return;
		}

		if (this.textStream.CanWrite)
		{
			this.textStream.BeginWrite(Encoding.UTF8.GetBytes(text), 0, text.Length, this.FinishAppend, DateTime.Now.Ticks);
		}
		else
		{
			Debug.LogError("Text Stream for logger " + this.name + "cannot write!");
		}
	}

	private void FinishAppend(IAsyncResult result) 
	{
		if (errored)
		{
			return;
		}

		if (result.IsCompleted)
		{
			this.textStream.EndWrite(result);
			
			if (this.writeQueue.Count > 0)
			{
				string nextText = this.writeQueue.Dequeue();
				this.BeginWrite(nextText);
			}
			else
			{
				isWriting = false;

				if (!isFileWriting)
				{
					this.DumpLogToFile();
				}
			}
		}
		else
		{
			Debug.LogError("Logger " + this.name + " has called FinishAppend but the AsyncResult is not Completed.");
		}
	}

	private async void DumpLogToFile()
	{
		if (errored)
		{
			return;
		}

		isFileWriting = true;
		Memory<byte> memory = new Memory<byte>();
		try 
		{
			int readLength = await this.textStream.ReadAsync(memory);
			await this.writer.WriteAsync(memory);
		}
		catch (Exception e)
		{
			Debug.LogException(e);
		}
		memory = null;
		await this.writer.FlushAsync();
		await this.textStream.FlushAsync();
		isFileWriting = false;
	}

	public void Dispose()
	{
		if (this.textStream != null)
		{
			this.textStream.Dispose();
		}
		
		if (this.writer != null)
		{
			this.writer.Dispose();
		}
	}
}
