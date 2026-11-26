using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;

public class RegistryBase<T>
{
	public readonly string registryName;
	private readonly Dictionary<string, T> registry = new Dictionary<string, T>();

	public RegistryBase(string registryName) 
	{ 
		this.registryName = registryName;
	}

	/// <summary>
	/// Adds a value <typeparamref name="T"/> into the registry for the given <paramref name="key"/>. 
	/// 
	/// All keys are case-insensitive and must only contain alphanumeric characters and underscores.
	/// </summary>
	/// <param name="key"></param>
	/// <param name="value"></param>
	/// <exception cref="ArgumentException"></exception>
	public void Add(string key, T value)
	{
		key = key.ToUpper();
		foreach (char c in key)
		{
			if (!char.IsLetterOrDigit(c) && c != '_')
			{
				throw new ArgumentException("Registry names can only contain uppercase alphanumeric and underscores.");
			}
		}

		if (registry.Keys.Contains(key)) 
		{
			throw new ArgumentException("Registry with the name " + key + " has already been registered in the " + this.registryName + " registry.");
		}

		try
		{ 
			registry.Add(key, value);
		}
		catch (Exception e)
		{
			Logger.MainLogger.AppendAsync(e.Message + Environment.NewLine + '\t' + e.ToString());
			Debug.LogException(e);
		}
	}

	public string GetKey(T value)
	{
		if (value == null)
		{
			return null;
		}

		return this.registry.FirstOrDefault(x => x.Value.Equals(value)).Key;
	}

	public Dictionary<string, T>.KeyCollection GetKeys() {
		return this.registry.Keys;
	}

	public T GetValue(string key)
	{
		return this.registry.GetValueOrDefault(key.ToUpper());
	}

	public int Size()
	{
		return this.registry.Count;
	}
}
