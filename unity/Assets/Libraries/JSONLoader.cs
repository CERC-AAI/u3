using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;

public class JSONLoader<T> where T : JSONLoader<T>, new()
{
	static public Dictionary<int, T> mDataList = new Dictionary<int, T>();

	static public void init(string directoryName)
	{
        DirectoryInfo info = new DirectoryInfo("Assets/Resources/Data/" + directoryName);
        FileInfo[] files = info.GetFiles();
        foreach (FileInfo file in files)
        {
            if (!file.Name.Contains(".meta"))
            {
                TextAsset data = (TextAsset)Resources.Load("Data/" + directoryName + "/" + file.Name.Replace(".json", ""));

                if (data != null)
                {
                    JSONObject tempObject = new JSONObject(data.text);

                    T tempData = new T();
                    tempData.loadFromJSONData(tempObject);
                }
                else
                {
                    Debug.LogError("Could not load event: " + file.Name);
                }
            }
        }
	}

	static public T getData(int ID)
	{
		if (mDataList.ContainsKey(ID))
		{
			return mDataList[ID];
		}
		else
		{
			//SQDebug.log("Couldn't find ID: " + ID);
			return null;
		}
	}

	virtual protected void loadFromJSONData(JSONObject jsonObject)
	{
	}

	virtual protected void addToDataMap(T newData)
	{
	}
}
