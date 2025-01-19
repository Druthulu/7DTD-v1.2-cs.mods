﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using UnityEngine;
using UnityEngine.Profiling;

public static class ThreadManager
{
	public static event Action UpdateEv;

	public static event Action LateUpdateEv;

	public static void ReleaseTaskInfo(ThreadManager.TaskInfo _info)
	{
		if (((_info != null) ? _info.evStopped : null) != null)
		{
			_info.evStopped.Close();
			_info.evStopped = null;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static ThreadManager.ThreadInfo startThread(string _name, ThreadManager.ThreadFunctionDelegate _threadDelegate, ThreadManager.ThreadFunctionDelegate _threadInit, ThreadManager.ThreadFunctionLoopDelegate _threadLoop, ThreadManager.ThreadFunctionEndDelegate _threadEnd, System.Threading.ThreadPriority _threadPriority, object _parameter, ThreadManager.ExitCallbackThread _exitCallback, bool _useRealThread = false, bool _isSilent = false)
	{
		ThreadManager.ThreadInfo threadInfo = new ThreadManager.ThreadInfo();
		threadInfo.parameter = _parameter;
		threadInfo.threadDelegate = _threadDelegate;
		threadInfo.threadInit = _threadInit;
		threadInfo.threadLoop = _threadLoop;
		threadInfo.threadEnd = _threadEnd;
		threadInfo.exitCallback = _exitCallback;
		threadInfo.isSilent = _isSilent;
		Dictionary<string, ThreadManager.ThreadInfo> activeThreads = ThreadManager.ActiveThreads;
		lock (activeThreads)
		{
			if (ThreadManager.ActiveThreads.ContainsKey(_name))
			{
				int num = 0;
				string text;
				do
				{
					num++;
					text = _name + num.ToString();
				}
				while (ThreadManager.ActiveThreads.ContainsKey(text));
				_name = text;
			}
			ThreadManager.ActiveThreads.Add(_name, threadInfo);
		}
		threadInfo.name = _name;
		if (_useRealThread)
		{
			Thread thread = new Thread(new ParameterizedThreadStart(ThreadManager.myThreadInvoke));
			thread.Name = _name;
			thread.Start(threadInfo);
			threadInfo.thread = thread;
		}
		else
		{
			ThreadPool.UnsafeQueueUserWorkItem(new WaitCallback(ThreadManager.myThreadInvoke), threadInfo);
		}
		return threadInfo;
	}

	public static ThreadManager.ThreadInfo StartThread(ThreadManager.ThreadFunctionDelegate _threadStart, System.Threading.ThreadPriority _threadPriority, object _parameter = null, ThreadManager.ExitCallbackThread _exitCallback = null, bool _useRealThread = false, bool _isSilent = false)
	{
		return ThreadManager.StartThread(_threadStart.Method.Name, _threadStart, _threadPriority, _parameter, _exitCallback, _useRealThread, _isSilent);
	}

	public static ThreadManager.ThreadInfo StartThread(string _name, ThreadManager.ThreadFunctionDelegate _threadDelegate, System.Threading.ThreadPriority _threadPriority, object _parameter = null, ThreadManager.ExitCallbackThread _exitCallback = null, bool _useRealThread = false, bool _isSilent = false)
	{
		return ThreadManager.startThread(_name, _threadDelegate, null, null, null, _threadPriority, _parameter, _exitCallback, _useRealThread, _isSilent);
	}

	public static ThreadManager.ThreadInfo StartThread(string _name, ThreadManager.ThreadFunctionDelegate _threadInit, ThreadManager.ThreadFunctionLoopDelegate _threadLoop, ThreadManager.ThreadFunctionEndDelegate _threadEnd, System.Threading.ThreadPriority _threadPriority = System.Threading.ThreadPriority.Normal, object _parameter = null, ThreadManager.ExitCallbackThread _exitCallback = null, bool _useRealThread = false, bool _isSilent = false)
	{
		return ThreadManager.startThread(_name, null, _threadInit, _threadLoop, _threadEnd, _threadPriority, _parameter, _exitCallback, _useRealThread, _isSilent);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void myThreadInvoke(object _threadInfo)
	{
		ThreadManager.ThreadInfo threadInfo = (ThreadManager.ThreadInfo)_threadInfo;
		CustomSampler.Create("ThreadDelegate", false);
		if (!threadInfo.isSilent)
		{
			Log.Out("Started thread " + threadInfo.name);
		}
		Exception e = null;
		try
		{
			if (threadInfo.threadDelegate != null)
			{
				threadInfo.threadDelegate(threadInfo);
			}
			else
			{
				if (threadInfo.threadInit != null)
				{
					threadInfo.threadInit(threadInfo);
				}
				bool exitForException = false;
				try
				{
					int num;
					do
					{
						num = threadInfo.threadLoop(threadInfo);
						if (num > 0)
						{
							Thread.Sleep(num);
						}
					}
					while (num >= 0);
				}
				catch (Exception ex)
				{
					Log.Error("Exception in thread {0}:", new object[]
					{
						threadInfo.name
					});
					Log.Exception(ex);
					e = ex;
					exitForException = true;
				}
				if (threadInfo.threadEnd != null)
				{
					threadInfo.threadEnd(threadInfo, exitForException);
				}
			}
		}
		catch (Exception ex2)
		{
			Log.Error("Exception in thread {0}:", new object[]
			{
				threadInfo.name
			});
			Log.Exception(ex2);
			e = ex2;
		}
		finally
		{
			if (!threadInfo.isSilent)
			{
				Log.Out("Exited thread " + threadInfo.name);
			}
			Dictionary<string, ThreadManager.ThreadInfo> activeThreads = ThreadManager.ActiveThreads;
			lock (activeThreads)
			{
				ThreadManager.ActiveThreads.Remove(threadInfo.name);
			}
			threadInfo.evStopped.Set();
		}
		if (threadInfo.exitCallback != null)
		{
			threadInfo.exitCallback(threadInfo, e);
		}
		Profiler.EndThreadProfiling();
	}

	public static ThreadManager.TaskInfo AddSingleTask(ThreadManager.TaskFunctionDelegate _taskDelegate, object _parameter = null, ThreadManager.ExitCallbackTask _exitCallback = null, bool _endEvent = true)
	{
		ThreadManager.TaskInfo taskInfo = new ThreadManager.TaskInfo(_endEvent);
		taskInfo.taskDelegate = _taskDelegate;
		taskInfo.parameter = _parameter;
		taskInfo.exitCallback = _exitCallback;
		taskInfo.name = _taskDelegate.Method.Name;
		ThreadPool.UnsafeQueueUserWorkItem(ThreadManager.queuedTaskDelegate, taskInfo);
		return taskInfo;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void myQueuedTaskInvoke(object _taskInfo)
	{
		ThreadManager.TaskInfo taskInfo = (ThreadManager.TaskInfo)_taskInfo;
		object obj = ThreadManager.lockObjectQueuedCounter;
		lock (obj)
		{
			ThreadManager.QueuedCount++;
		}
		Exception e = null;
		try
		{
			taskInfo.taskDelegate(taskInfo);
		}
		catch (Exception ex)
		{
			Log.Error("Exception in task");
			Log.Exception(ex);
			e = ex;
		}
		finally
		{
			if (taskInfo.evStopped != null)
			{
				taskInfo.evStopped.Set();
			}
		}
		obj = ThreadManager.lockObjectQueuedCounter;
		lock (obj)
		{
			ThreadManager.QueuedCount--;
		}
		if (taskInfo.exitCallback != null)
		{
			taskInfo.exitCallback(taskInfo, e);
		}
		Profiler.EndThreadProfiling();
	}

	public static void AddSingleTaskMainThread(string _name, ThreadManager.MainThreadTaskFunctionDelegate _func, object _parameter = null)
	{
		ThreadManager.MainThreadTaskInfo item = default(ThreadManager.MainThreadTaskInfo);
		item.taskDelegate = _func;
		item.parameter = _parameter;
		item.name = _name;
		object obj = ThreadManager.lockObjectMainThreadTasks;
		lock (obj)
		{
			ThreadManager.mainThreadTasks.Add(item);
		}
	}

	public static void UpdateMainThreadTasks()
	{
		Action updateEv = ThreadManager.UpdateEv;
		if (updateEv != null)
		{
			updateEv();
		}
		int count = ThreadManager.mainThreadTasks.Count;
		if (count == 0)
		{
			return;
		}
		object obj = ThreadManager.lockObjectMainThreadTasks;
		lock (obj)
		{
			List<ThreadManager.MainThreadTaskInfo> list = ThreadManager.mainThreadTasks;
			ThreadManager.mainThreadTasks = ThreadManager.mainThreadTasksCopy;
			ThreadManager.mainThreadTasksCopy = list;
		}
		count = ThreadManager.mainThreadTasksCopy.Count;
		for (int i = 0; i < count; i++)
		{
			try
			{
				ThreadManager.MainThreadTaskInfo mainThreadTaskInfo = ThreadManager.mainThreadTasksCopy[i];
				mainThreadTaskInfo.taskDelegate(mainThreadTaskInfo.parameter);
			}
			catch (Exception e)
			{
				Log.Exception(e);
			}
		}
		ThreadManager.mainThreadTasksCopy.Clear();
	}

	public static void LateUpdate()
	{
		Action lateUpdateEv = ThreadManager.LateUpdateEv;
		if (lateUpdateEv == null)
		{
			return;
		}
		lateUpdateEv();
	}

	public static void Shutdown()
	{
		Log.Out("Terminating threads");
		using (Dictionary<string, ThreadManager.ThreadInfo>.Enumerator enumerator = ThreadManager.ActiveThreads.GetEnumerator())
		{
			while (enumerator.MoveNext())
			{
				KeyValuePair<string, ThreadManager.ThreadInfo> keyValuePair = enumerator.Current;
				keyValuePair.Value.RequestTermination();
			}
			goto IL_80;
		}
		IL_44:
		using (Dictionary<string, ThreadManager.ThreadInfo>.Enumerator enumerator = ThreadManager.ActiveThreads.GetEnumerator())
		{
			if (enumerator.MoveNext())
			{
				KeyValuePair<string, ThreadManager.ThreadInfo> keyValuePair2 = enumerator.Current;
				keyValuePair2.Value.WaitForEnd();
			}
		}
		IL_80:
		if (ThreadManager.ActiveThreads.Count <= 0)
		{
			return;
		}
		goto IL_44;
	}

	public static void SetMonoBehaviour(MonoBehaviour _monoBehaviour)
	{
		ThreadManager.monoBehaviour = _monoBehaviour;
	}

	public static Coroutine StartCoroutine(IEnumerator _e)
	{
		if (ThreadManager.IsMainThread())
		{
			return ThreadManager.monoBehaviour.StartCoroutine(_e);
		}
		ThreadManager.AddSingleTaskMainThread("Coroutine", delegate(object _taskInfo)
		{
			ThreadManager.StartCoroutine(_e);
		}, null);
		return null;
	}

	public static void StopCoroutine(IEnumerator _e)
	{
		ThreadManager.monoBehaviour.StopCoroutine(_e);
	}

	public static void StopCoroutine(Coroutine _coroutine)
	{
		ThreadManager.monoBehaviour.StopCoroutine(_coroutine);
	}

	public static void StopCoroutine(string _methodName)
	{
		ThreadManager.monoBehaviour.StopCoroutine(_methodName);
	}

	public static void RunCoroutine(IEnumerator _e, Action _iterCallback)
	{
		while (_e.MoveNext())
		{
			object obj = _e.Current;
			IEnumerator enumerator = obj as IEnumerator;
			if (enumerator != null)
			{
				ThreadManager.RunCoroutine(enumerator, _iterCallback);
			}
			else
			{
				_iterCallback();
			}
		}
	}

	public static bool IsInSyncCoroutine
	{
		get
		{
			return ThreadManager.syncCoroutineNestingLevel > 0;
		}
	}

	public static void RunCoroutineSync(IEnumerator _func)
	{
		ThreadManager.syncCoroutineNestingLevel++;
		try
		{
			while (_func.MoveNext())
			{
				object obj = _func.Current;
				IEnumerator enumerator = obj as IEnumerator;
				if (enumerator != null)
				{
					ThreadManager.RunCoroutineSync(enumerator);
				}
			}
		}
		finally
		{
			ThreadManager.syncCoroutineNestingLevel--;
		}
	}

	public static IEnumerator CoroutineWrapperWithExceptionCallback(IEnumerator _enumerator, Action<Exception> _exceptionHandler)
	{
		Stack<IEnumerator> stack = new Stack<IEnumerator>();
		stack.Push(_enumerator);
		while (stack.Count > 0)
		{
			IEnumerator enumerator = stack.Peek();
			object obj;
			try
			{
				if (!enumerator.MoveNext())
				{
					stack.Pop();
					continue;
				}
				obj = enumerator.Current;
			}
			catch (Exception obj2)
			{
				_exceptionHandler(obj2);
				yield break;
			}
			IEnumerator enumerator2 = obj as IEnumerator;
			if (enumerator2 != null)
			{
				stack.Push(enumerator2);
			}
			else
			{
				yield return obj;
			}
		}
		yield break;
	}

	public static void SetMainThreadRef(Thread _mainThreadRef)
	{
		ThreadManager.MainThreadRef = _mainThreadRef;
		ThreadManager.MainThreadId = _mainThreadRef.ManagedThreadId;
	}

	public static bool IsMainThread()
	{
		return Thread.CurrentThread.ManagedThreadId == ThreadManager.MainThreadId;
	}

	[Conditional("DEBUG")]
	[PublicizedFrom(EAccessModifier.Private)]
	public static void DebugLog(string _messagePart1, string _messagePart2 = null)
	{
	}

	public const int cEndTime = -1;

	[PublicizedFrom(EAccessModifier.Private)]
	public const int threadTerminationTimeout = 30;

	public static Thread MainThreadRef;

	[PublicizedFrom(EAccessModifier.Private)]
	public static int MainThreadId;

	public static Dictionary<string, ThreadManager.ThreadInfo> ActiveThreads = new Dictionary<string, ThreadManager.ThreadInfo>();

	public static int QueuedCount;

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly object lockObjectQueuedCounter = new object();

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly object lockObjectMainThreadTasks = new object();

	[PublicizedFrom(EAccessModifier.Private)]
	public static List<ThreadManager.MainThreadTaskInfo> mainThreadTasks = new List<ThreadManager.MainThreadTaskInfo>(150);

	[PublicizedFrom(EAccessModifier.Private)]
	public static List<ThreadManager.MainThreadTaskInfo> mainThreadTasksCopy = new List<ThreadManager.MainThreadTaskInfo>(150);

	[PublicizedFrom(EAccessModifier.Private)]
	public static MonoBehaviour monoBehaviour;

	[PublicizedFrom(EAccessModifier.Private)]
	public static WaitCallback queuedTaskDelegate = new WaitCallback(ThreadManager.myQueuedTaskInvoke);

	[PublicizedFrom(EAccessModifier.Private)]
	public static int syncCoroutineNestingLevel;

	public class ThreadInfo
	{
		public void RequestTermination()
		{
			this.evRunning.Set();
		}

		public bool TerminationRequested()
		{
			return this.evRunning.WaitOne(0);
		}

		public bool HasTerminated()
		{
			return this.evStopped.WaitOne(0);
		}

		public void WaitForEnd()
		{
			this.RequestTermination();
			if (!this.evStopped.WaitOne(30000))
			{
				Log.Error(string.Concat(new string[]
				{
					"Thread ",
					this.name,
					" did not finish within ",
					30.ToString(),
					"s. Request trace: ",
					StackTraceUtility.ExtractStackTrace()
				}));
				Thread thread = this.thread;
				if (thread == null)
				{
					return;
				}
				thread.Abort();
			}
		}

		public object parameter;

		public ThreadManager.ThreadFunctionDelegate threadDelegate;

		public ThreadManager.ThreadFunctionDelegate threadInit;

		public ThreadManager.ThreadFunctionLoopDelegate threadLoop;

		public ThreadManager.ThreadFunctionEndDelegate threadEnd;

		public string name;

		public Thread thread;

		public bool isSilent;

		public readonly ManualResetEvent evRunning = new ManualResetEvent(false);

		public readonly ManualResetEvent evStopped = new ManualResetEvent(false);

		public ThreadManager.ExitCallbackThread exitCallback;

		public object threadData;
	}

	public class TaskInfo
	{
		public TaskInfo(bool _endEvent = true)
		{
			if (_endEvent)
			{
				this.evStopped = new ManualResetEvent(false);
			}
		}

		public void WaitForEnd()
		{
			this.evStopped.WaitOne();
		}

		public string name;

		public ThreadManager.TaskFunctionDelegate taskDelegate;

		public object parameter;

		public ThreadManager.ExitCallbackTask exitCallback;

		public ManualResetEvent evStopped;
	}

	public struct MainThreadTaskInfo
	{
		public string name;

		public ThreadManager.MainThreadTaskFunctionDelegate taskDelegate;

		public object parameter;
	}

	public delegate int ThreadFunctionLoopDelegate(ThreadManager.ThreadInfo _threadInfo);

	public delegate void ThreadFunctionEndDelegate(ThreadManager.ThreadInfo _threadInfo, bool _exitForException);

	public delegate void ThreadFunctionDelegate(ThreadManager.ThreadInfo _threadInfo);

	public delegate void TaskFunctionDelegate(ThreadManager.TaskInfo _taskInfo);

	public delegate void MainThreadTaskFunctionDelegate(object _parameter);

	public delegate void ExitCallbackThread(ThreadManager.ThreadInfo _ti, Exception _e);

	public delegate void ExitCallbackTask(ThreadManager.TaskInfo _ti, Exception _e);
}
