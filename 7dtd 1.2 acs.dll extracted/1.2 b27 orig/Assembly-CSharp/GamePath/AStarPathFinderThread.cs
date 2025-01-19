using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

namespace GamePath
{
	public class AStarPathFinderThread : PathFinderThread
	{
		public AStarPathFinderThread()
		{
			PathFinderThread.Instance = this;
		}

		public override int GetFinishedCount()
		{
			return this.finishedPaths.Count;
		}

		public override int GetQueueCount()
		{
			return this.entityWaitQueue.list.Count;
		}

		public override void StartWorkerThreads()
		{
			this.threadInfo = ThreadManager.StartThread("Pathfinder", null, new ThreadManager.ThreadFunctionLoopDelegate(this.thread_Pathfinder), null, System.Threading.ThreadPriority.Lowest, null, null, false, false);
		}

		public override void Cleanup()
		{
			this.threadInfo.RequestTermination();
			this.writerThreadWaitHandle.Set();
			this.threadInfo.WaitForEnd();
			this.threadInfo = null;
			this.entityWaitQueue.Clear();
			this.finishedPaths.Clear();
			this.writerThreadWaitHandle = null;
		}

		[PublicizedFrom(EAccessModifier.Protected)]
		public int thread_Pathfinder(ThreadManager.ThreadInfo _threadInfo)
		{
			while (!_threadInfo.TerminationRequested())
			{
				try
				{
					if (this.entityWaitQueue.list.Count == 0)
					{
						this.writerThreadWaitHandle.WaitOne();
					}
					PathInfo pathInfo = PathInfo.Empty;
					Dictionary<int, PathInfo> obj = this.finishedPaths;
					lock (obj)
					{
						if (this.entityWaitQueue.list.Count <= 0)
						{
							continue;
						}
						int num = this.entityWaitQueue.list[0];
						this.entityWaitQueue.Remove(num);
						if (!this.finishedPaths.ContainsKey(num))
						{
							continue;
						}
						pathInfo = this.finishedPaths[num];
					}
					pathInfo.entity.navigator.GetPathTo(pathInfo);
					obj = this.finishedPaths;
					lock (obj)
					{
						if (pathInfo.path == null)
						{
							this.finishedPaths.Remove(pathInfo.entity.entityId);
						}
						else
						{
							this.finishedPaths[pathInfo.entity.entityId] = pathInfo;
						}
					}
				}
				catch (Exception ex)
				{
					Log.Error("Exception in PathFinder thread: " + ex.Message);
					Log.Error(ex.StackTrace);
				}
			}
			return -1;
		}

		public override bool IsCalculatingPath(int _entityId)
		{
			Dictionary<int, PathInfo> obj = this.finishedPaths;
			bool result;
			lock (obj)
			{
				result = this.finishedPaths.ContainsKey(_entityId);
			}
			return result;
		}

		public override void FindPath(EntityAlive _entity, Vector3 _target, float _speed, bool _canBreak, EAIBase _aiTask)
		{
			Dictionary<int, PathInfo> obj = this.finishedPaths;
			lock (obj)
			{
				if (!this.entityWaitQueue.hashSet.Contains(_entity.entityId))
				{
					this.entityWaitQueue.Add(_entity.entityId);
				}
				this.finishedPaths[_entity.entityId] = new PathInfo(_entity, _target, _canBreak, _speed, _aiTask);
			}
			this.writerThreadWaitHandle.Set();
		}

		public override PathInfo GetPath(int _entityId)
		{
			Dictionary<int, PathInfo> obj = this.finishedPaths;
			lock (obj)
			{
				PathInfo pathInfo;
				if (this.finishedPaths.TryGetValue(_entityId, out pathInfo) && pathInfo.path != null)
				{
					this.finishedPaths.Remove(_entityId);
					return pathInfo;
				}
			}
			return PathInfo.Empty;
		}

		public override void RemovePathsFor(int _entityId)
		{
			Dictionary<int, PathInfo> obj = this.finishedPaths;
			lock (obj)
			{
				this.finishedPaths.Remove(_entityId);
				if (this.entityWaitQueue.hashSet.Contains(_entityId))
				{
					this.entityWaitQueue.Remove(_entityId);
				}
			}
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public ThreadManager.ThreadInfo threadInfo;

		[PublicizedFrom(EAccessModifier.Private)]
		public AutoResetEvent writerThreadWaitHandle = new AutoResetEvent(false);

		[PublicizedFrom(EAccessModifier.Private)]
		public HashSetList<int> entityWaitQueue = new HashSetList<int>();

		[PublicizedFrom(EAccessModifier.Private)]
		public Dictionary<int, PathInfo> finishedPaths = new Dictionary<int, PathInfo>();
	}
}
