using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;

public abstract class XmlLoader : MonoBehaviour {
	
	public string resourceName;
	
	protected XmlDocument xmlDoc;
	
	protected bool xmlAvailable = false;
	
	protected delegate void xmlMutator();
	
	void Start () {
		if (Application.isPlaying) {
			load();
			xmlAvailable = true;
		}
	}
	
	protected IEnumerator withLock(xmlMutator f) {
		while (!xmlAvailable) {
			yield return 0;
		}
		xmlAvailable = false;
		f();
		xmlAvailable = true;
	}
	
	protected void assertReady() {
		if (!xmlAvailable) {
			throw new MissingReferenceException("xml file " + resourceName + " is not ready");
		}
	}
	
	private void load() {
		xmlDoc = new XmlDocument();
		MemoryStream ms = new MemoryStream(docBytes());
		xmlDoc.Load(ms);
		ms.Close();
	}
	
	protected virtual byte[] docBytes() {
		TextAsset textDoc = (TextAsset)Resources.Load(resourceName);
		return textDoc.bytes;
	}
	
	protected T getDatum<T>(string xPath, System.Func<XmlNode, T> f) {
		assertReady();
		XmlNode xn = xmlDoc.SelectSingleNode(xPath);
		return f(xn);
	}
	
	protected IEnumerable<T> getData<T>(string xPath, System.Func<XmlNode, T> f) {
		assertReady();
		XmlNodeList xnl = xmlDoc.SelectNodes(xPath);
		return xnl.Cast<XmlNode>().Select<XmlNode, T>(f);
	}
}