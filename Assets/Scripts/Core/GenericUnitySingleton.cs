using UnityEngine;

namespace Runtime
{
	public class GenericUnitySingleton<T> : MonoBehaviour where T : MonoBehaviour
	{
		protected static T instance;
		private static readonly object lockObject = new object();
		private static bool applicationIsQuitting = false;

		public static T Instance
		{
			get
			{
				if (applicationIsQuitting)
				{
					Debug.LogWarning(
						$"[Singleton] Instance '{typeof(T)}' already destroyed on application quit. Won't create again.");
					return null;
				}

				lock (lockObject)
				{
					if (instance == null)
					{
						instance = (T) FindObjectOfType(typeof(T));

						if (FindObjectsOfType(typeof(T)).Length > 1)
						{
							Debug.LogError(
								$"[Singleton] Something went really wrong — there should never be more than one singleton of type {typeof(T)}!");
							return instance;
						}

						if (instance == null)
						{
							GameObject singletonObject = new GameObject();
							instance = singletonObject.AddComponent<T>();
							singletonObject.name = $"(singleton) {typeof(T)}";

							DontDestroyOnLoad(singletonObject);
							Debug.Log($"[Singleton] An instance of {typeof(T)} is created with DontDestroyOnLoad.");
						}
						else
						{
							Debug.Log(
								$"[Singleton] Using existing instance of {typeof(T)}: {instance.gameObject.name}");
						}
					}

					return instance;
				}
			}
		}

		protected virtual void OnDestroy()
		{
			applicationIsQuitting = true;
		}
	}
}