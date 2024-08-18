using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.Rendering;

namespace MeshCombineStudio
{
	[ExecuteInEditMode]
	public class MeshCombineJobManager : MonoBehaviour
	{
		[Serializable]
		public class JobSettings
		{
			public CombineJobMode combineJobMode;

			public ThreadAmountMode threadAmountMode;

			public int combineMeshesPerFrame = 4;

			public bool useMultiThreading = true;

			public bool useMainThread = true;

			public int customThreadAmount = 1;

			public bool showStats;

			public void CopySettings(JobSettings source)
			{
				combineJobMode = source.combineJobMode;
				threadAmountMode = source.threadAmountMode;
				combineMeshesPerFrame = source.combineMeshesPerFrame;
				useMultiThreading = source.useMultiThreading;
				useMainThread = source.useMainThread;
				customThreadAmount = source.customThreadAmount;
			}

			public void ReportStatus()
			{
				Debug.Log("---------------------");
				Debug.Log("combineJobMode " + combineJobMode);
				Debug.Log("threadAmountMode " + threadAmountMode);
				Debug.Log("combineMeshesPerFrame " + combineMeshesPerFrame);
				Debug.Log("useMultiThreading " + useMultiThreading);
				Debug.Log("useMainThread " + useMainThread);
				Debug.Log("customThreadAmount " + customThreadAmount);
			}
		}

		public enum CombineJobMode
		{
			CombineAtOnce = 0,
			CombinePerFrame = 1
		}

		public enum ThreadAmountMode
		{
			AllThreads = 0,
			HalfThreads = 1,
			Custom = 2
		}

		public enum ThreadState
		{
			isReady = 0,
			isRunning = 1,
			hasError = 2
		}

		public class MeshCombineJobsThread
		{
			public int threadId;

			public ThreadState threadState;

			public Queue<MeshCombineJob> meshCombineJobs = new Queue<MeshCombineJob>();

			public MeshCombineJobsThread(int threadId)
			{
				this.threadId = threadId;
			}

			public void ExecuteJobsThread(object state)
			{
				threadState = ThreadState.isRunning;
				NewMeshObject newMeshObject = null;
				try
				{
					while (true)
					{
						newMeshObject = null;
						bool flag = false;
						int num = Interlocked.Increment(ref instance.totalNewMeshObjects);
						if (instance.jobSettings.combineJobMode == CombineJobMode.CombinePerFrame)
						{
							if (num > instance.jobSettings.combineMeshesPerFrame)
							{
								flag = true;
							}
						}
						else if (num > instance.threadAmount)
						{
							flag = true;
						}
						if (flag)
						{
							Interlocked.Decrement(ref instance.totalNewMeshObjects);
							break;
						}
						MeshCombineJob meshCombineJob;
						lock (meshCombineJobs)
						{
							if (meshCombineJobs.Count == 0)
							{
								break;
							}
							meshCombineJob = meshCombineJobs.Dequeue();
							goto IL_00b8;
						}
						IL_00b8:
						lock (instance.newMeshObjectsPool)
						{
							if (instance.newMeshObjectsPool.Count == 0)
							{
								newMeshObject = new NewMeshObject();
							}
							else
							{
								newMeshObject = instance.newMeshObjectsPool[instance.newMeshObjectsPool.Count - 1];
								instance.newMeshObjectsPool.RemoveAt(instance.newMeshObjectsPool.Count - 1);
							}
						}
						newMeshObject.newPosition = meshCombineJob.position;
						newMeshObject.Combine(meshCombineJob);
						lock (instance.newMeshObjectsDoneThread)
						{
							instance.newMeshObjectsDoneThread.Enqueue(newMeshObject);
						}
					}
				}
				catch (Exception ex)
				{
					if (newMeshObject != null)
					{
						lock (instance.newMeshObjectsPool)
						{
							instance.newMeshObjectsPool.Add(newMeshObject);
						}
						Interlocked.Decrement(ref instance.totalNewMeshObjects);
					}
					lock (meshCombineJobs)
					{
						meshCombineJobs.Clear();
					}
					Interlocked.Decrement(ref instance.totalNewMeshObjects);
					Debug.LogError("Mesh Combine Studio thread error -> " + ex.ToString());
					threadState = ThreadState.hasError;
					return;
				}
				threadState = ThreadState.isReady;
			}
		}

		public class MeshCombineJob
		{
			public MeshCombiner meshCombiner;

			public MeshObjectsHolder meshObjectsHolder;

			public Transform parent;

			public Vector3 position;

			public int startIndex;

			public int endIndex;

			public bool firstMesh;

			public bool intersectsSurface;

			public int backFaceTrianglesRemoved;

			public int trianglesRemoved;

			public MeshCombineJob(MeshCombiner meshCombiner, MeshObjectsHolder meshObjectsHolder, Transform parent, Vector3 position, int startIndex, int length, bool firstMesh, bool intersectsSurface)
			{
				this.meshCombiner = meshCombiner;
				this.meshObjectsHolder = meshObjectsHolder;
				this.parent = parent;
				this.position = position;
				this.startIndex = startIndex;
				this.firstMesh = firstMesh;
				this.intersectsSurface = intersectsSurface;
				endIndex = startIndex + length;
				meshObjectsHolder.lodParent.jobsPending++;
				meshCombiner.jobCount++;
			}
		}

		public class NewMeshObject
		{
			public MeshCombineJob meshCombineJob;

			public MeshCache.SubMeshCache newMeshCache = new MeshCache.SubMeshCache();

			public bool allSkipped;

			public Vector3 newPosition;

			private byte[] vertexIsBelow;

			private readonly byte belowSurface = 1;

			private readonly byte aboveSurface = 2;

			public NewMeshObject()
			{
				newMeshCache.Init();
			}

			public void Combine(MeshCombineJob meshCombineJob)
			{
				this.meshCombineJob = meshCombineJob;
				int startIndex = meshCombineJob.startIndex;
				int endIndex = meshCombineJob.endIndex;
				List<MeshObject> meshObjects = meshCombineJob.meshObjectsHolder.meshObjects;
				newMeshCache.ResetHasBooleans();
				int num = 0;
				int num2 = 0;
				int num3 = endIndex - startIndex;
				MeshCombiner meshCombiner = meshCombineJob.meshCombiner;
				bool validCopyBakedLighting = meshCombiner.validCopyBakedLighting;
				bool validRebakeLighting = meshCombiner.validRebakeLighting;
				bool flag = meshCombiner.rebakeLightingMode == MeshCombiner.RebakeLightingMode.RegenarateLightmapUvs;
				int num4 = 0;
				int num5 = 0;
				int num6 = 0;
				float num7 = 0f;
				if (validRebakeLighting)
				{
					num4 = Mathf.CeilToInt(Mathf.Sqrt(num3));
					num7 = 1f / (float)num4;
				}
				allSkipped = true;
				for (int i = startIndex; i < endIndex; i++)
				{
					MeshObject meshObject = meshObjects[i];
					if (meshObject.skip)
					{
						continue;
					}
					allSkipped = false;
					MeshCache meshCache = meshObject.meshCache;
					int subMeshIndex = meshObject.subMeshIndex;
					MeshCache.SubMeshCache subMeshCache = meshCache.subMeshCache[subMeshIndex];
					Vector3 vector = meshObject.position - meshCombineJob.position;
					Quaternion rotation = meshObject.rotation;
					Vector3 scale = meshObject.scale;
					bool flag2 = false;
					if (scale.x < 0f)
					{
						flag2 = !flag2;
					}
					if (scale.y < 0f)
					{
						flag2 = !flag2;
					}
					if (scale.z < 0f)
					{
						flag2 = !flag2;
					}
					Vector3[] vertices = subMeshCache.vertices;
					Vector3[] normals = subMeshCache.normals;
					Vector4[] tangents = subMeshCache.tangents;
					Vector2[] uv = subMeshCache.uv;
					Vector2[] uv2 = subMeshCache.uv2;
					Vector2[] uv3 = subMeshCache.uv3;
					Vector2[] uv4 = subMeshCache.uv4;
					Color32[] colors = subMeshCache.colors32;
					int[] triangles = subMeshCache.triangles;
					int vertexCount = subMeshCache.vertexCount;
					int[] triangles2 = newMeshCache.triangles;
					HasArray(ref newMeshCache.hasNormals, subMeshCache.hasNormals, ref newMeshCache.normals, normals, vertexCount, num);
					HasArray(ref newMeshCache.hasTangents, subMeshCache.hasTangents, ref newMeshCache.tangents, tangents, vertexCount, num);
					HasArray(ref newMeshCache.hasUv, subMeshCache.hasUv, ref newMeshCache.uv, uv, vertexCount, num);
					HasArray(ref newMeshCache.hasUv2, subMeshCache.hasUv2, ref newMeshCache.uv2, uv2, vertexCount, num);
					HasArray(ref newMeshCache.hasUv3, subMeshCache.hasUv3, ref newMeshCache.uv3, uv3, vertexCount, num);
					HasArray(ref newMeshCache.hasUv4, subMeshCache.hasUv4, ref newMeshCache.uv4, uv4, vertexCount, num);
					HasArray(ref newMeshCache.hasColors, subMeshCache.hasColors, ref newMeshCache.colors32, colors, vertexCount, num);
					Vector3[] vertices2 = newMeshCache.vertices;
					Vector3[] normals2 = newMeshCache.normals;
					Vector4[] tangents2 = newMeshCache.tangents;
					Vector2[] uv5 = newMeshCache.uv;
					Vector2[] uv6 = newMeshCache.uv2;
					Vector2[] uv7 = newMeshCache.uv3;
					Vector2[] uv8 = newMeshCache.uv4;
					Color32[] colors2 = newMeshCache.colors32;
					bool hasNormals = subMeshCache.hasNormals;
					bool hasTangents = subMeshCache.hasTangents;
					for (int j = 0; j < vertices.Length; j++)
					{
						int num8 = j + num;
						vertices2[num8] = rotation * new Vector3(vertices[j].x * scale.x, vertices[j].y * scale.y, vertices[j].z * scale.z) + vector;
						if (hasNormals)
						{
							normals2[num8] = rotation * normals[j];
						}
						if (hasTangents)
						{
							tangents2[num8] = rotation * tangents[j];
							tangents2[num8].w = tangents[j].w;
						}
					}
					if (subMeshCache.hasUv)
					{
						Array.Copy(uv, 0, uv5, num, vertexCount);
					}
					if (subMeshCache.hasUv2)
					{
						if (validCopyBakedLighting)
						{
							Vector4 lightmapScaleOffset = meshObject.lightmapScaleOffset;
							Vector2 vector2 = new Vector2(lightmapScaleOffset.z, lightmapScaleOffset.w);
							Vector2 vector3 = new Vector2(lightmapScaleOffset.x, lightmapScaleOffset.y);
							for (int k = 0; k < vertices.Length; k++)
							{
								int num9 = k + num;
								uv6[num9] = new Vector2(uv2[k].x * vector3.x, uv2[k].y * vector3.y) + vector2;
							}
						}
						else if (validRebakeLighting)
						{
							if (!flag)
							{
								Vector2 vector4 = new Vector2(num7 * (float)num5, num7 * (float)num6);
								for (int l = 0; l < vertices.Length; l++)
								{
									int num10 = l + num;
									uv6[num10] = uv2[l] * num7 + vector4;
								}
							}
						}
						else
						{
							Array.Copy(uv2, 0, uv6, num, vertexCount);
						}
					}
					if (subMeshCache.hasUv3)
					{
						Array.Copy(uv3, 0, uv7, num, vertexCount);
					}
					if (subMeshCache.hasUv4)
					{
						Array.Copy(uv4, 0, uv8, num, vertexCount);
					}
					if (subMeshCache.hasColors)
					{
						Array.Copy(colors, 0, colors2, num, vertexCount);
					}
					if (flag2)
					{
						for (int m = 0; m < triangles.Length; m += 3)
						{
							triangles2[m + num2] = triangles[m + 2] + num;
							triangles2[m + num2 + 1] = triangles[m + 1] + num;
							triangles2[m + num2 + 2] = triangles[m] + num;
						}
					}
					else
					{
						for (int n = 0; n < triangles.Length; n++)
						{
							triangles2[n + num2] = triangles[n] + num;
						}
					}
					num += vertexCount;
					num2 += triangles.Length;
					if (++num5 >= num4)
					{
						num5 = 0;
						num6++;
					}
				}
				newMeshCache.vertexCount = num;
				newMeshCache.triangleCount = num2;
				if (meshCombiner.removeBackFaceTriangles)
				{
					RemoveBackFaceTriangles();
				}
			}

			private void HasArray<T>(ref bool hasNewArray, bool hasArray, ref T[] newArray, Array array, int vertexCount, int totalVertices)
			{
				if (hasArray)
				{
					if (!hasNewArray)
					{
						if (newArray == null)
						{
							newArray = new T[65534];
						}
						else
						{
							Array.Clear(newArray, 0, totalVertices);
						}
					}
					hasNewArray = true;
				}
				else if (hasNewArray)
				{
					Array.Clear(newArray, totalVertices, vertexCount);
				}
			}

			public void RemoveTrianglesBelowSurface(Transform t, MeshCombineJob meshCombineJob)
			{
				if (vertexIsBelow == null)
				{
					vertexIsBelow = new byte[65534];
				}
				Ray ray = instance.ray;
				RaycastHit hitInfo = instance.hitInfo;
				Vector3 zero = Vector3.zero;
				int layerMask = meshCombineJob.meshCombiner.surfaceLayerMask;
				float num = meshCombineJob.meshCombiner.maxSurfaceHeight;
				Vector3[] vertices = newMeshCache.vertices;
				int[] triangles = newMeshCache.triangles;
				List<MeshObject> meshObjects = meshCombineJob.meshObjectsHolder.meshObjects;
				int startIndex = meshCombineJob.startIndex;
				int endIndex = meshCombineJob.endIndex;
				for (int i = startIndex; i < endIndex; i++)
				{
					MeshObject meshObject = meshObjects[i];
					if (!meshObject.intersectsSurface)
					{
						continue;
					}
					int startNewTriangleIndex = meshObject.startNewTriangleIndex;
					int num2 = meshObject.newTriangleCount + startNewTriangleIndex;
					for (int j = startNewTriangleIndex; j < num2; j += 3)
					{
						bool flag = false;
						for (int k = 0; k < 3; k++)
						{
							int num3 = triangles[j + k];
							if (num3 == -1)
							{
								continue;
							}
							byte b = vertexIsBelow[num3];
							if (b == 0)
							{
								zero = t.TransformPoint(vertices[num3]);
								ray.origin = new Vector3(zero.x, num, zero.z);
								if (!Physics.Raycast(ray, out hitInfo, num - zero.y, layerMask))
								{
									vertexIsBelow[num3] = aboveSurface;
									flag = true;
									break;
								}
								b = ((!(zero.y < hitInfo.point.y)) ? aboveSurface : belowSurface);
								vertexIsBelow[num3] = b;
							}
							if (b != belowSurface)
							{
								flag = true;
								break;
							}
						}
						if (!flag)
						{
							meshCombineJob.trianglesRemoved += 3;
							triangles[j] = -1;
						}
					}
				}
				Array.Clear(vertexIsBelow, 0, newMeshCache.vertexCount);
			}

			public void RemoveBackFaceTriangles()
			{
				int[] triangles = newMeshCache.triangles;
				Vector3[] normals = newMeshCache.normals;
				int triangleCount = newMeshCache.triangleCount;
				MeshCombiner meshCombiner = meshCombineJob.meshCombiner;
				bool flag = meshCombiner.backFaceTriangleMode == MeshCombiner.BackFaceTriangleMode.Direction;
				Bounds backFaceBounds = meshCombiner.backFaceBounds;
				Vector3 min = backFaceBounds.min;
				Vector3 max = backFaceBounds.max;
				Vector3[] vertices = newMeshCache.vertices;
				Vector3 lhs = Quaternion.Euler(meshCombiner.backFaceDirection) * Vector3.forward;
				Vector3 vector = default(Vector3);
				for (int i = 0; i < triangleCount; i += 3)
				{
					Vector3 zero = Vector3.zero;
					Vector3 zero2 = Vector3.zero;
					for (int j = 0; j < 3; j++)
					{
						int num = triangles[i + j];
						zero2 += vertices[num];
						zero += normals[num];
					}
					zero2 /= 3f;
					zero /= 3f;
					if (!flag)
					{
						vector.x = ((!(zero.x > 0f)) ? min.x : max.x);
						vector.y = ((!(zero.y > 0f)) ? min.y : max.y);
						vector.z = ((!(zero.z > 0f)) ? min.z : max.z);
						lhs = newPosition + zero2 - vector;
					}
					if (Vector3.Dot(lhs, zero) >= 0f)
					{
						triangles[i] = -1;
						meshCombineJob.backFaceTrianglesRemoved += 3;
					}
				}
			}

			private void ArrangeTriangles()
			{
				int num = newMeshCache.triangleCount;
				int[] triangles = newMeshCache.triangles;
				for (int i = 0; i < num; i += 3)
				{
					if (triangles[i] == -1)
					{
						triangles[i] = triangles[num - 3];
						triangles[i + 1] = triangles[num - 2];
						triangles[i + 2] = triangles[num - 1];
						i -= 3;
						num -= 3;
					}
				}
				newMeshCache.triangleCount = num;
			}

			public void CreateMesh()
			{
				MeshCombiner meshCombiner = meshCombineJob.meshCombiner;
				if (meshCombiner.instantiatePrefab == null)
				{
					Debug.LogError("Mesh Combine Studio -> Instantiate Prefab = null");
					return;
				}
				MeshObjectsHolder meshObjectsHolder = meshCombineJob.meshObjectsHolder;
				GameObject gameObject = UnityEngine.Object.Instantiate(meshCombiner.instantiatePrefab, newPosition, Quaternion.identity, meshCombineJob.parent);
				string name2 = (gameObject.name = meshCombineJob.meshObjectsHolder.mat.name);
				gameObject.layer = meshCombiner.outputLayer;
				Mesh mesh = new Mesh();
				mesh.name = name2;
				if (meshCombineJob.intersectsSurface)
				{
					RemoveTrianglesBelowSurface(gameObject.transform, meshCombineJob);
				}
				if (meshCombineJob.trianglesRemoved > 0 || meshCombineJob.backFaceTrianglesRemoved > 0)
				{
					ArrangeTriangles();
					if (instance.tempMeshCache == null)
					{
						instance.tempMeshCache = new MeshCache.SubMeshCache();
						instance.tempMeshCache.Init(false);
					}
					instance.tempMeshCache.CopySubMeshCache(newMeshCache);
					newMeshCache.RebuildVertexBuffer(instance.tempMeshCache, false);
				}
				int vertexCount = newMeshCache.vertexCount;
				int triangleCount = newMeshCache.triangleCount;
				meshCombiner.totalVertices += vertexCount;
				meshCombiner.totalTriangles += triangleCount;
				MeshExtension.ApplyVertices(mesh, newMeshCache.vertices, vertexCount);
				MeshExtension.ApplyTriangles(mesh, newMeshCache.triangles, triangleCount);
				if (newMeshCache.hasNormals)
				{
					MeshExtension.ApplyNormals(mesh, newMeshCache.normals, vertexCount);
				}
				if (newMeshCache.hasTangents)
				{
					MeshExtension.ApplyTangents(mesh, newMeshCache.tangents, vertexCount);
				}
				if (newMeshCache.hasUv)
				{
					MeshExtension.ApplyUvs(mesh, newMeshCache.uv, 0, vertexCount);
				}
				if (newMeshCache.hasUv2)
				{
					MeshExtension.ApplyUvs(mesh, newMeshCache.uv2, 1, vertexCount);
				}
				if (newMeshCache.hasUv3)
				{
					MeshExtension.ApplyUvs(mesh, newMeshCache.uv3, 2, vertexCount);
				}
				if (newMeshCache.hasUv4)
				{
					MeshExtension.ApplyUvs(mesh, newMeshCache.uv4, 3, vertexCount);
				}
				if (newMeshCache.hasColors)
				{
					MeshExtension.ApplyColors32(mesh, newMeshCache.colors32, vertexCount);
				}
				CachedComponents component = gameObject.GetComponent<CachedComponents>();
				if (meshCombiner.addMeshColliders)
				{
					MeshCollider meshCollider = gameObject.AddComponent<MeshCollider>();
					meshCollider.sharedMesh = mesh;
				}
				if (meshCombiner.makeMeshesUnreadable)
				{
					mesh.UploadMeshData(true);
				}
				meshCombiner.newDrawCalls++;
				component.mr.sharedMaterial = meshCombineJob.meshObjectsHolder.mat;
				component.mf.sharedMesh = mesh;
				component.mr.lightmapIndex = meshObjectsHolder.lightmapIndex;
				component.garbageCollectMesh.mesh = mesh;
				if (meshCombineJob.meshObjectsHolder.shadowCastingModeTwoSided || (meshCombiner.twoSidedShadows && meshCombineJob.backFaceTrianglesRemoved > 0))
				{
					component.mr.shadowCastingMode = ShadowCastingMode.TwoSided;
				}
				if (meshObjectsHolder.newCachedGOs == null)
				{
					meshObjectsHolder.newCachedGOs = new List<CachedGameObject>();
				}
				meshObjectsHolder.newCachedGOs.Add(new CachedGameObject(component));
				meshObjectsHolder.lodParent.lodLevels[meshObjectsHolder.lodLevel].newMeshRenderers.Add(component.mr);
				if (--meshObjectsHolder.lodParent.jobsPending == 0)
				{
					meshObjectsHolder.lodParent.AssignLODGroup(meshCombiner);
				}
			}
		}

		public static MeshCombineJobManager instance;

		public JobSettings jobSettings = new JobSettings();

		[NonSerialized]
		public List<NewMeshObject> newMeshObjectsPool = new List<NewMeshObject>();

		public Dictionary<Mesh, MeshCache> meshCacheDictionary = new Dictionary<Mesh, MeshCache>();

		[NonSerialized]
		public int totalNewMeshObjects;

		public Queue<NewMeshObject> newMeshObjectsDone = new Queue<NewMeshObject>();

		public Queue<NewMeshObject> newMeshObjectsDoneThread = new Queue<NewMeshObject>();

		public Queue<MeshCombineJob> meshCombineJobs = new Queue<MeshCombineJob>();

		public MeshCombineJobsThread[] meshCombineJobsThreads;

		public int cores;

		public int threadAmount;

		public int startThreadId;

		public int endThreadId;

		public bool abort;

		private MeshCache.SubMeshCache tempMeshCache;

		private Ray ray = new Ray(Vector3.zero, Vector3.down);

		private RaycastHit hitInfo;

		public static MeshCombineJobManager CreateInstance(MeshCombiner meshCombiner, GameObject instantiatePrefab)
		{
			if (instance != null)
			{
				return instance;
			}
			GameObject gameObject = new GameObject("MCS Job Manager");
			instance = gameObject.AddComponent<MeshCombineJobManager>();
			instance.SetJobMode(meshCombiner.jobSettings);
			return instance;
		}

		private void Awake()
		{
			instance = this;
		}

		private void OnEnable()
		{
			instance = this;
			base.gameObject.hideFlags = HideFlags.DontSave | HideFlags.HideInHierarchy;
			Init();
		}

		public void Init()
		{
			cores = Environment.ProcessorCount;
			if (meshCombineJobsThreads == null || meshCombineJobsThreads.Length != cores)
			{
				meshCombineJobsThreads = new MeshCombineJobsThread[cores];
				for (int i = 0; i < meshCombineJobsThreads.Length; i++)
				{
					meshCombineJobsThreads[i] = new MeshCombineJobsThread(i);
				}
			}
		}

		private void OnDisable()
		{
		}

		private void OnDestroy()
		{
			AbortJobs();
			instance = null;
		}

		private void Update()
		{
			if (Application.isPlaying)
			{
				MyUpdate();
			}
		}

		private void MyUpdate()
		{
			ExecuteJobs();
			if (newMeshObjectsDone.Count > 0 || newMeshObjectsDoneThread.Count > 0)
			{
				CombineMeshesDone();
			}
		}

		public void SetJobMode(JobSettings newJobSettings)
		{
			if (newJobSettings.combineMeshesPerFrame < 1)
			{
				Debug.LogError("MCS Job Manager -> CombineMeshesPerFrame is " + newJobSettings.combineMeshesPerFrame + " and should be 1 or higher.");
				return;
			}
			if (newJobSettings.combineMeshesPerFrame > 128)
			{
				Debug.LogError("MCS Job Manager -> CombineMeshesPerFrame is " + newJobSettings.combineMeshesPerFrame + " and should be 128 or lower.");
				return;
			}
			if (newJobSettings.customThreadAmount < 1)
			{
				Debug.LogError("MCS Job Manager -> customThreadAmount is " + newJobSettings.combineMeshesPerFrame + " and should be 1 or higher.");
				return;
			}
			if (newJobSettings.customThreadAmount > cores)
			{
				newJobSettings.customThreadAmount = cores;
			}
			jobSettings.CopySettings(newJobSettings);
			if (jobSettings.useMultiThreading)
			{
				startThreadId = ((!jobSettings.useMainThread) ? 1 : 0);
				if (jobSettings.threadAmountMode == ThreadAmountMode.Custom)
				{
					if (jobSettings.customThreadAmount > cores - startThreadId)
					{
						jobSettings.customThreadAmount = cores - startThreadId;
					}
					threadAmount = jobSettings.customThreadAmount;
				}
				else
				{
					if (jobSettings.threadAmountMode == ThreadAmountMode.AllThreads)
					{
						threadAmount = cores;
					}
					else
					{
						threadAmount = cores / 2;
					}
					threadAmount -= startThreadId;
				}
				endThreadId = startThreadId + threadAmount;
			}
			else
			{
				startThreadId = 0;
				endThreadId = 1;
				threadAmount = 1;
			}
			int num = ((jobSettings.combineJobMode != CombineJobMode.CombinePerFrame) ? threadAmount : jobSettings.combineMeshesPerFrame);
			while (newMeshObjectsPool.Count > num)
			{
				newMeshObjectsPool.RemoveAt(newMeshObjectsPool.Count - 1);
			}
		}

		public void AddJob(MeshCombiner meshCombiner, MeshObjectsHolder meshObjectsHolder, Transform parent, Vector3 position)
		{
			List<MeshObject> meshObjects = meshObjectsHolder.meshObjects;
			int num = 0;
			int num2 = 0;
			int startIndex = 0;
			int num3 = 0;
			bool firstMesh = true;
			bool intersectsSurface = false;
			Mesh mesh = null;
			MeshCache value = null;
			int num4 = ((!meshCombiner.useVertexOutputLimit) ? 65534 : meshCombiner.vertexOutputLimit);
			for (int i = 0; i < meshObjects.Count; i++)
			{
				MeshObject meshObject = meshObjects[i];
				meshObject.skip = false;
				meshCombiner.originalDrawCalls++;
				Mesh mesh2 = meshObject.cachedGO.mesh;
				if (mesh2 != mesh && !meshCacheDictionary.TryGetValue(mesh2, out value))
				{
					value = new MeshCache(mesh2);
					meshCacheDictionary.Add(mesh2, value);
				}
				mesh = mesh2;
				meshObject.meshCache = value;
				int vertexCount = value.subMeshCache[meshObject.subMeshIndex].vertexCount;
				int triangleCount = value.subMeshCache[meshObject.subMeshIndex].triangleCount;
				meshCombiner.originalTotalVertices += vertexCount;
				meshCombiner.originalTotalTriangles += triangleCount;
				if (num + vertexCount > num4)
				{
					meshCombineJobs.Enqueue(new MeshCombineJob(meshCombiner, meshObjectsHolder, parent, position, startIndex, num3, firstMesh, intersectsSurface));
					firstMesh = (intersectsSurface = false);
					num = (num2 = (num3 = 0));
					startIndex = i;
				}
				if (meshCombiner.removeTrianglesBelowSurface)
				{
					int num5 = MeshIntersectsSurface(meshCombiner, meshObject.cachedGO);
					if (num5 == 0)
					{
						intersectsSurface = (meshObject.intersectsSurface = true);
						meshObject.startNewTriangleIndex = num2;
						meshObject.newTriangleCount = triangleCount;
						meshObject.skip = false;
					}
					else
					{
						meshObject.intersectsSurface = false;
						if (num5 == -1)
						{
							meshObject.skip = true;
							num3++;
							continue;
						}
						meshObject.skip = false;
					}
				}
				num += vertexCount;
				num2 += triangleCount;
				num3++;
			}
			if (num > 0)
			{
				meshCombineJobs.Enqueue(new MeshCombineJob(meshCombiner, meshObjectsHolder, parent, position, startIndex, num3, firstMesh, intersectsSurface));
			}
		}

		public int MeshIntersectsSurface(MeshCombiner meshCombiner, CachedGameObject cachedGO)
		{
			MeshRenderer mr = cachedGO.mr;
			LayerMask surfaceLayerMask = meshCombiner.surfaceLayerMask;
			int maxSurfaceHeight = meshCombiner.maxSurfaceHeight;
			if (Physics.CheckBox(mr.bounds.center, mr.bounds.extents, Quaternion.identity, surfaceLayerMask))
			{
				return 0;
			}
			Vector3 min = mr.bounds.min;
			float maxDistance = (float)meshCombiner.maxSurfaceHeight - min.y;
			ray.origin = new Vector3(min.x, maxSurfaceHeight, min.z);
			if (Physics.Raycast(ray, out hitInfo, maxDistance, surfaceLayerMask) && min.y < hitInfo.point.y)
			{
				return -1;
			}
			return 1;
		}

		public void AbortJobs()
		{
			foreach (MeshCombineJob meshCombineJob in meshCombineJobs)
			{
				meshCombineJob.meshCombiner.jobCount = 0;
			}
			meshCombineJobs.Clear();
			for (int i = 0; i < meshCombineJobsThreads.Length; i++)
			{
				MeshCombineJobsThread meshCombineJobsThread = meshCombineJobsThreads[i];
				lock (meshCombineJobsThread.meshCombineJobs)
				{
					meshCombineJobsThread.meshCombineJobs.Clear();
				}
			}
			newMeshObjectsPool.Clear();
			totalNewMeshObjects = 0;
			abort = true;
		}

		public void ExecuteJobs()
		{
			while (meshCombineJobs.Count > 0)
			{
				int num = 999999;
				int num2 = 0;
				for (int i = startThreadId; i < endThreadId; i++)
				{
					int count = meshCombineJobsThreads[i].meshCombineJobs.Count;
					if (count < num)
					{
						num2 = i;
						num = count;
						if (num == 0)
						{
							break;
						}
					}
				}
				lock (meshCombineJobsThreads[num2].meshCombineJobs)
				{
					meshCombineJobsThreads[num2].meshCombineJobs.Enqueue(meshCombineJobs.Dequeue());
				}
			}
			try
			{
				bool flag;
				do
				{
					flag = false;
					if (jobSettings.useMultiThreading)
					{
						for (int j = 1; j < endThreadId; j++)
						{
							MeshCombineJobsThread meshCombineJobsThread = meshCombineJobsThreads[j];
							bool flag2 = false;
							lock (meshCombineJobsThread.meshCombineJobs)
							{
								if (meshCombineJobsThread.meshCombineJobs.Count > 0)
								{
									flag2 = true;
								}
							}
							if (flag2)
							{
								flag = true;
								if (meshCombineJobsThread.threadState == ThreadState.isReady)
								{
									ThreadPool.QueueUserWorkItem(meshCombineJobsThread.ExecuteJobsThread);
								}
								else if (meshCombineJobsThread.threadState == ThreadState.hasError)
								{
									AbortJobs();
									return;
								}
							}
						}
					}
					if ((!jobSettings.useMultiThreading || jobSettings.useMainThread) && meshCombineJobsThreads[0].meshCombineJobs.Count > 0)
					{
						flag = true;
						meshCombineJobsThreads[0].ExecuteJobsThread(null);
					}
					if (jobSettings.combineJobMode == CombineJobMode.CombineAtOnce)
					{
						CombineMeshesDone();
					}
				}
				while (jobSettings.combineJobMode == CombineJobMode.CombineAtOnce && flag);
			}
			catch (Exception ex)
			{
				Debug.LogError("Mesh Combine Studio error -> " + ex.ToString());
				AbortJobs();
			}
		}

		public void CombineMeshesDone()
		{
			lock (newMeshObjectsDoneThread)
			{
				while (newMeshObjectsDoneThread.Count > 0)
				{
					newMeshObjectsDone.Enqueue(newMeshObjectsDoneThread.Dequeue());
				}
			}
			int num = 0;
			while (newMeshObjectsDone.Count > 0)
			{
				NewMeshObject newMeshObject = newMeshObjectsDone.Dequeue();
				if (!abort && !newMeshObject.allSkipped)
				{
					newMeshObject.CreateMesh();
				}
				MeshCombiner meshCombiner = newMeshObject.meshCombineJob.meshCombiner;
				if (--meshCombiner.jobCount == 0)
				{
					meshCombiner.ExecuteOnCombiningReady();
				}
				lock (newMeshObjectsPool)
				{
					newMeshObjectsPool.Add(newMeshObject);
				}
				Interlocked.Decrement(ref totalNewMeshObjects);
				if (jobSettings.combineJobMode == CombineJobMode.CombinePerFrame && ++num > jobSettings.combineMeshesPerFrame && !abort)
				{
					break;
				}
			}
			abort = false;
		}
	}
}
