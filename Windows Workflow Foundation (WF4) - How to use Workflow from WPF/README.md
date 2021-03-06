# Windows Workflow Foundation (WF4) - How to use Workflow from WPF
## Requires
- Visual Studio 2010
## License
- Apache License, Version 2.0
## Technologies
- Windows Workflow
- WPF
- WF
- WF4
- Windows Presentation Foundation
## Topics
- Workflow
- Progress Bar
## Updated
- 09/29/2011
## Description

<h1>How to use Workflow from WPF MVVM</h1>
<p>In this sample I'll show you what I learned about using WPF and WF together. I started by listing the requirements for the client application</p>
<ul>
<li>Follows MVVM architecture </li><li>Runs workflows on background thread in the client process </li><li>Supports notification of events from Workflow with a Progress Bar </li><li>Supports control of workflow from UI (Cancel) </li><li>High test coverage </li><li>Implemented in TDD style (Test First - Red / Green / Refactor) </li></ul>
<h2>Challenges</h2>
<p>There are several challenges I needed to overcome to implement this solution.</p>
<ul>
<li>How to use a Workflow in a WPF app built with MVVM architecture </li><li>How to support notifications from a workflow without messaging activities </li><li>How to control a workflow from the client app in MVVM </li></ul>
<p>And what does this application do? It counts numbers. Yes, pointless I know but the example is just there to demonstrate a process (any process) controlled by a workflow from a WPF application.</p>
<p><img src="41724-counting.png" alt="" width="525" height="240"></p>
<h2>How to use a Workflow in a WPF app built with MVVM architecture</h2>
<p>Workflows can run in any managed process. Many times they run as a service using IIS and Windows Server AppFabric. In this sample I want to run the workflow in the same process as the WPF app.</p>
<p>In the MVVM architecture the logical place to do this is in the model. When I started building the application, I started with the model writing the test first and then implementing the model. Doing this helped me to get very high test coverage (nearly 90%)
 on the model.</p>
<h3>What is the purpose of the model?</h3>
<p>The job of the model in MVVM is to represent the data and the processes that are exposed to the UI. The model knows nothing about the UI that it will be consumed by. This separation allows me to focus on creating an object that encapsulates the necessary
 business logic and data access code in a highly testable fashion.</p>
<p>To start with I created the <strong>ICountModel </strong>interface to describe the capabilities I wanted in the model
<br>
<img src="41725-icountmodel.png" alt="" width="234" height="279"></p>
<p>Rather than use events, I created properties of Action&lt;T&gt; that allow the tests or View Model to receive notifications from the model about events in the workflow lifecycle. The
<strong>ICountModel</strong> interface is implemented by the <strong>internal</strong>
<strong>CountModel </strong>class. I made this class internal because I want the View Model to use ICountModel and not the CountModel class.</p>
<h2>How to support notifications from a workflow without messaging activities</h2>
<p>Workflows can communicate with other apps and workflows using messaging activities. In my case I would be running the workflow within the host application's AppDomain so this allows me to take advantage of extensions.</p>
<p>The WorkflowApplication.Extensions property contains a collection of Extensions which are simply objects (or functions that create objects) that you stuff in there. This is how we will pass the model to the activities that will notify the host about events
 in the workflow.</p>
<p>The workflow accepts two arguments</p>
<table>
<tbody>
<tr>
<td valign="top">
<p><strong>Name</strong></p>
</td>
<td valign="top">
<p><strong>Type</strong></p>
</td>
<td valign="top">
<p><strong>Description</strong></p>
</td>
</tr>
<tr>
<td valign="top">
<p>CountTo</p>
</td>
<td valign="top">
<p>Int32</p>
</td>
<td valign="top">
<p>The number you want to count to</p>
</td>
</tr>
<tr>
<td valign="top">
<p>CountDelay</p>
</td>
<td valign="top">
<p>Int32</p>
</td>
<td valign="top">
<p>The number of milliseconds you want to delay after eachcount</p>
</td>
</tr>
</tbody>
</table>
<p><img src="41726-countwf.png" alt="" width="767" height="477"></p>
<p>As you can see in the workflow I've created a number of custom activities that do notifications when the count starts, completes, cancels and updates. To handle the case where the workflow is canceled I use a CancellationScope activity which allows me to
 specify a CancellationHandler for the notification.</p>
<p>The notification activities are surprisingly simple</p>
<p>&nbsp;</p>
<div class="scriptcode">
<div class="pluginEditHolder" pluginCommand="mceScriptCode">
<div class="title"><span>C#</span></div>
<div class="pluginLinkHolder"><span class="pluginEditHolderLink">Edit</span>|<span class="pluginRemoveHolderLink">Remove</span></div>
<span class="hidden">csharp</span>

<div class="preview">
<pre class="csharp"><span class="cs__keyword">public</span>&nbsp;<span class="cs__keyword">sealed</span>&nbsp;<span class="cs__keyword">class</span>&nbsp;NotifyCountStarted&nbsp;:&nbsp;NativeActivity&nbsp;
{&nbsp;
&nbsp;&nbsp;&nbsp;&nbsp;<span class="cs__keyword">protected</span>&nbsp;<span class="cs__keyword">override</span>&nbsp;<span class="cs__keyword">void</span>&nbsp;CacheMetadata(NativeActivityMetadata&nbsp;metadata)&nbsp;
&nbsp;&nbsp;&nbsp;&nbsp;{&nbsp;
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;metadata.RequireExtension&lt;ICountModel&gt;();&nbsp;
&nbsp;&nbsp;&nbsp;&nbsp;}&nbsp;
&nbsp;
&nbsp;&nbsp;&nbsp;&nbsp;<span class="cs__keyword">protected</span>&nbsp;<span class="cs__keyword">override</span>&nbsp;<span class="cs__keyword">void</span>&nbsp;Execute(NativeActivityContext&nbsp;context)&nbsp;
&nbsp;&nbsp;&nbsp;&nbsp;{&nbsp;
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;var&nbsp;countModel&nbsp;=&nbsp;context.GetExtension&lt;ICountModel&gt;();&nbsp;
&nbsp;
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;<span class="cs__keyword">if</span>&nbsp;(countModel.CountStarted&nbsp;!=&nbsp;<span class="cs__keyword">null</span>)&nbsp;
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;{&nbsp;
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;countModel.CountStarted();&nbsp;
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;}&nbsp;
&nbsp;&nbsp;&nbsp;&nbsp;}&nbsp;
}&nbsp;
</pre>
</div>
</div>
</div>
<div class="endscriptcode">&nbsp;The activity obtains the model from the extensions collection and if the notification delegate is provided the activity invokes it.</div>
<p>&nbsp;</p>
<h3>Starting the Workflow</h3>
<p>When I want to invoke the workflow I have to add the extension before running the workflow. In this case, the model is the extension so I simply add &quot;this&quot;.</p>
<p>&nbsp;</p>
<div class="scriptcode">
<div class="pluginEditHolder" pluginCommand="mceScriptCode">
<div class="title"><span>C#</span></div>
<div class="pluginLinkHolder"><span class="pluginEditHolderLink">Edit</span>|<span class="pluginRemoveHolderLink">Remove</span></div>
<span class="hidden">csharp</span>

<div class="preview">
<pre class="csharp"><span class="cs__keyword">public</span>&nbsp;<span class="cs__keyword">void</span>&nbsp;StartCounting(<span class="cs__keyword">int</span>&nbsp;count&nbsp;=&nbsp;<span class="cs__number">100</span>,&nbsp;<span class="cs__keyword">int</span>&nbsp;delay&nbsp;=&nbsp;<span class="cs__number">50</span>)&nbsp;
{&nbsp;
&nbsp;&nbsp;&nbsp;&nbsp;<span class="cs__keyword">this</span>.countTo&nbsp;=&nbsp;count;&nbsp;
&nbsp;&nbsp;&nbsp;&nbsp;<span class="cs__keyword">this</span>.countDelay&nbsp;=&nbsp;delay;&nbsp;
&nbsp;
&nbsp;&nbsp;&nbsp;&nbsp;<span class="cs__keyword">this</span>.workflow&nbsp;=&nbsp;<span class="cs__keyword">new</span>&nbsp;WorkflowApplication(&nbsp;
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;<span class="cs__keyword">new</span>&nbsp;WorkflowCount&nbsp;
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;{&nbsp;
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;CountTo&nbsp;=&nbsp;<span class="cs__keyword">this</span>.countTo,&nbsp;
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;CountDelay&nbsp;=&nbsp;<span class="cs__keyword">this</span>.countDelay&nbsp;
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;})&nbsp;{&nbsp;Aborted&nbsp;=&nbsp;<span class="cs__keyword">this</span>.OnAborted&nbsp;};&nbsp;
&nbsp;
&nbsp;&nbsp;&nbsp;&nbsp;<span class="cs__keyword">this</span>.workflow.Extensions.Add(<span class="cs__keyword">this</span>);&nbsp;
&nbsp;&nbsp;&nbsp;&nbsp;<span class="cs__keyword">if</span>&nbsp;(<span class="cs__keyword">this</span>.extension&nbsp;!=&nbsp;<span class="cs__keyword">null</span>)&nbsp;
&nbsp;&nbsp;&nbsp;&nbsp;{&nbsp;
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;<span class="cs__keyword">this</span>.workflow.Extensions.Add(<span class="cs__keyword">this</span>.extension);&nbsp;
&nbsp;&nbsp;&nbsp;&nbsp;}&nbsp;
&nbsp;&nbsp;&nbsp;&nbsp;<span class="cs__keyword">this</span>.workflow.Run();&nbsp;
}&nbsp;
</pre>
</div>
</div>
</div>
<div class="endscriptcode">&nbsp;</div>
<p>&nbsp;</p>
<h2>Canceling the Workflow</h2>
<p>To cancel the workflow there are two cases. In one case we want to cancel the workflow synchronously as in the case where the user clicked the button to cancel.</p>
<p>&nbsp;</p>
<div class="scriptcode">
<div class="pluginEditHolder" pluginCommand="mceScriptCode">
<div class="title"><span>C#</span></div>
<div class="pluginLinkHolder"><span class="pluginEditHolderLink">Edit</span>|<span class="pluginRemoveHolderLink">Remove</span></div>
<span class="hidden">csharp</span>

<div class="preview">
<pre class="js">public&nbsp;<span class="js__operator">void</span>&nbsp;CancelCounting()&nbsp;
<span class="js__brace">{</span>&nbsp;
&nbsp;&nbsp;&nbsp;&nbsp;workflow.Cancel();&nbsp;
<span class="js__brace">}</span>&nbsp;
</pre>
</div>
</div>
</div>
<div class="endscriptcode">The other case is when the WPF window is closing. In this case we need to cancel the workflow but we want to do it asynchronously. If you cancel synchronously in the Window.Closing event the window will appear to freeze momentarily
 when closing. Instead what you want is to start the cancel process in Window.Closing and then finish it in the Window.Closed event. Usually the cancel will be complete by the time the Closed event is fired. Even if it isn't the window won't be visible while
 it tries to cancel.</div>
<p>&nbsp;</p>
<p>Even though the model doesn't need this capability, the ViewModel will need to handle the closing/close events and it doesn't have access to the workflow so the model will need to expose these methods.</p>
<p>&nbsp;</p>
<div class="scriptcode">
<div class="pluginEditHolder" pluginCommand="mceScriptCode">
<div class="title"><span>C#</span></div>
<div class="pluginLinkHolder"><span class="pluginEditHolderLink">Edit</span>|<span class="pluginRemoveHolderLink">Remove</span></div>
<span class="hidden">csharp</span>

<div class="preview">
<pre class="csharp"><span class="cs__keyword">public</span>&nbsp;IAsyncResult&nbsp;BeginCancelCounting(&nbsp;
&nbsp;&nbsp;&nbsp;&nbsp;AsyncCallback&nbsp;callback,&nbsp;<span class="cs__keyword">object</span>&nbsp;state)&nbsp;
{&nbsp;
&nbsp;&nbsp;&nbsp;&nbsp;<span class="cs__keyword">return</span>&nbsp;<span class="cs__keyword">this</span>.workflow&nbsp;!=&nbsp;<span class="cs__keyword">null</span>&nbsp;
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;?&nbsp;<span class="cs__keyword">this</span>.workflow.BeginCancel(callback,&nbsp;state)&nbsp;
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;:&nbsp;<span class="cs__keyword">null</span>;&nbsp;
}&nbsp;
&nbsp;
<span class="cs__keyword">public</span>&nbsp;<span class="cs__keyword">void</span>&nbsp;EndCancelCounting(IAsyncResult&nbsp;result)&nbsp;
{&nbsp;
&nbsp;&nbsp;&nbsp;&nbsp;<span class="cs__keyword">if</span>&nbsp;(workflow&nbsp;!=&nbsp;<span class="cs__keyword">null</span>&nbsp;&amp;&amp;&nbsp;result&nbsp;!=&nbsp;<span class="cs__keyword">null</span>)&nbsp;
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;workflow.EndCancel(result);&nbsp;
}&nbsp;
</pre>
</div>
</div>
</div>
<div class="endscriptcode">&nbsp;At this point there are two things that need testing. I built the tests as I went along and here is how I did it.</div>
<p>&nbsp;</p>
<h3>Testing the Notification Activities</h3>
<p>There are the following aspects to the notification activities that I want to test.</p>
<ol>
<li>Do they respond appropriately if the extension is not present (throwing an exception)?
</li><li>Do they raise the notification if the extension is present and the delegate is supplied?
</li><li>Do they fail if the extension is present but no delegate is supplied? </li></ol>
<p>For each of the Notification Activities I have tests that verify the behavior.</p>
<h4>Verifying that the activity throws an exception if the extension is not found</h4>
<p>&nbsp;</p>
<div class="scriptcode">
<div class="pluginEditHolder" pluginCommand="mceScriptCode">
<div class="title"><span>C#</span></div>
<div class="pluginLinkHolder"><span class="pluginEditHolderLink">Edit</span>|<span class="pluginRemoveHolderLink">Remove</span></div>
<span class="hidden">csharp</span>

<div class="preview">
<pre class="csharp">[TestMethod]&nbsp;
[ExpectedException(<span class="cs__keyword">typeof</span>(ValidationException))]&nbsp;
<span class="cs__keyword">public</span>&nbsp;<span class="cs__keyword">void</span>&nbsp;NotifyStartedShouldThrowOnNoExtension()&nbsp;
{&nbsp;
&nbsp;&nbsp;&nbsp;&nbsp;WorkflowInvoker.Invoke(<span class="cs__keyword">new</span>&nbsp;NotifyCountStarted());&nbsp;
}&nbsp;
</pre>
</div>
</div>
</div>
<h4 class="endscriptcode">&nbsp;Verifiying that the activity raises the notification if the extension is present and the delegate is supplied</h4>
<p class="endscriptcode">&nbsp;</p>
<div class="scriptcode">
<div class="pluginEditHolder" pluginCommand="mceScriptCode">
<div class="title"><span>C#</span></div>
<div class="pluginLinkHolder"><span class="pluginEditHolderLink">Edit</span>|<span class="pluginRemoveHolderLink">Remove</span></div>
<span class="hidden">csharp</span>

<div class="preview">
<pre class="csharp">[TestMethod]&nbsp;
<span class="cs__keyword">public</span>&nbsp;<span class="cs__keyword">void</span>&nbsp;NotifyUpdatedShouldNotify()&nbsp;
{&nbsp;
&nbsp;&nbsp;&nbsp;&nbsp;var&nbsp;model&nbsp;=&nbsp;CountModelFactory.CreateModel();&nbsp;
&nbsp;&nbsp;&nbsp;&nbsp;var&nbsp;updateNotified&nbsp;=&nbsp;<span class="cs__keyword">false</span>;&nbsp;
&nbsp;&nbsp;&nbsp;&nbsp;var&nbsp;actual&nbsp;=&nbsp;<span class="cs__number">0</span>;&nbsp;
&nbsp;&nbsp;&nbsp;&nbsp;<span class="cs__keyword">const</span>&nbsp;<span class="cs__keyword">int</span>&nbsp;expected&nbsp;=&nbsp;<span class="cs__number">32</span>;&nbsp;
&nbsp;
&nbsp;&nbsp;&nbsp;&nbsp;<span class="cs__com">//&nbsp;The&nbsp;delegate&nbsp;will&nbsp;be&nbsp;invoked&nbsp;with&nbsp;the&nbsp;count</span>&nbsp;
&nbsp;&nbsp;&nbsp;&nbsp;model.CountUpdated&nbsp;=&nbsp;i&nbsp;=&gt;&nbsp;
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;{&nbsp;
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;updateNotified&nbsp;=&nbsp;<span class="cs__keyword">true</span>;&nbsp;
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;actual&nbsp;=&nbsp;i;&nbsp;
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;};&nbsp;
&nbsp;
&nbsp;&nbsp;&nbsp;&nbsp;var&nbsp;invoker&nbsp;=&nbsp;<span class="cs__keyword">new</span>&nbsp;WorkflowInvoker(&nbsp;
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;<span class="cs__keyword">new</span>&nbsp;NotifyCountUpdated&nbsp;
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;{&nbsp;
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;CurrentCount&nbsp;=&nbsp;expected&nbsp;
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;});&nbsp;
&nbsp;&nbsp;&nbsp;&nbsp;invoker.Extensions.Add(model);&nbsp;
&nbsp;
&nbsp;&nbsp;&nbsp;&nbsp;invoker.Invoke();&nbsp;
&nbsp;
&nbsp;&nbsp;&nbsp;&nbsp;Assert.IsTrue(updateNotified);&nbsp;
&nbsp;&nbsp;&nbsp;&nbsp;Assert.AreEqual(expected,&nbsp;actual);&nbsp;
}&nbsp;
</pre>
</div>
</div>
</div>
<h4 class="endscriptcode">Verifying that the activity does not fail if the extension is present but no delegate is supplied</h4>
<p class="endscriptcode">&nbsp;</p>
<div class="scriptcode">
<div class="pluginEditHolder" pluginCommand="mceScriptCode">
<div class="title"><span>C#</span></div>
<div class="pluginLinkHolder"><span class="pluginEditHolderLink">Edit</span>|<span class="pluginRemoveHolderLink">Remove</span></div>
<span class="hidden">csharp</span>

<div class="preview">
<pre class="csharp">[TestMethod]&nbsp;
<span class="cs__keyword">public</span>&nbsp;<span class="cs__keyword">void</span>&nbsp;NotifyUpdatedWithNoDelegateShouldDoNothing()&nbsp;
{&nbsp;
&nbsp;&nbsp;&nbsp;&nbsp;ICountModel&nbsp;model&nbsp;=&nbsp;CountModelFactory.CreateModel();&nbsp;
&nbsp;&nbsp;&nbsp;&nbsp;WorkflowInvoker&nbsp;invoker&nbsp;=&nbsp;<span class="cs__keyword">new</span>&nbsp;WorkflowInvoker(<span class="cs__keyword">new</span>&nbsp;NotifyCountUpdated());&nbsp;
&nbsp;&nbsp;&nbsp;&nbsp;invoker.Extensions.Add(model);&nbsp;
&nbsp;
&nbsp;&nbsp;&nbsp;&nbsp;invoker.Invoke();&nbsp;
&nbsp;&nbsp;&nbsp;&nbsp;<span class="cs__com">//&nbsp;No&nbsp;exception&nbsp;is&nbsp;success</span>&nbsp;
}&nbsp;
</pre>
</div>
</div>
</div>
<h2 class="endscriptcode">Testing the Model</h2>
<p>&nbsp;</p>
<p>&nbsp;</p>
<p>&nbsp;</p>
<p>When testing the model I have to take into account the WorkflowApplication will run the workflow on a different thread. That means I will need to use synchronization objects to cause the test thread to wait. Creating the tests while I was developing the
 class forced me to deal with this reality the Action delegates really helped here.</p>
<p>&nbsp;</p>
<div class="scriptcode">
<div class="pluginEditHolder" pluginCommand="mceScriptCode">
<div class="title"><span>C#</span></div>
<div class="pluginLinkHolder"><span class="pluginEditHolderLink">Edit</span>|<span class="pluginRemoveHolderLink">Remove</span></div>
<span class="hidden">csharp</span>

<div class="preview">
<pre class="csharp">[TestMethod]&nbsp;
<span class="cs__keyword">public</span><span class="cs__keyword">void</span>&nbsp;ShouldRaiseCompleteEvent()&nbsp;
{&nbsp;
&nbsp;&nbsp;&nbsp;&nbsp;var&nbsp;tracking&nbsp;=&nbsp;<span class="cs__keyword">new</span>&nbsp;MemoryTrackingParticipant();&nbsp;
&nbsp;&nbsp;&nbsp;&nbsp;var&nbsp;target&nbsp;=&nbsp;CountModelFactory.CreateModel(tracking);&nbsp;
&nbsp;&nbsp;&nbsp;&nbsp;var&nbsp;countCompleted&nbsp;=&nbsp;<span class="cs__keyword">new</span>&nbsp;AutoResetEvent(<span class="cs__keyword">false</span>);&nbsp;
&nbsp;
&nbsp;&nbsp;&nbsp;&nbsp;target.CountCompleted&nbsp;=&nbsp;()&nbsp;=&gt;&nbsp;countCompleted.Set();&nbsp;
&nbsp;
&nbsp;&nbsp;&nbsp;&nbsp;<span class="cs__keyword">try</span>&nbsp;
&nbsp;&nbsp;&nbsp;&nbsp;{&nbsp;
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;target.StartCounting(<span class="cs__number">1</span>);&nbsp;
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;Assert.IsTrue(countCompleted.WaitOne(TestTimeout));&nbsp;
&nbsp;&nbsp;&nbsp;&nbsp;}&nbsp;
&nbsp;&nbsp;&nbsp;&nbsp;<span class="cs__keyword">finally</span>&nbsp;
&nbsp;&nbsp;&nbsp;&nbsp;{&nbsp;
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;tracking.Trace();&nbsp;
&nbsp;&nbsp;&nbsp;&nbsp;}&nbsp;
}&nbsp;
</pre>
</div>
</div>
</div>
<p>&nbsp;</p>
<h2>Implementing the ViewModel</h2>
<p>&nbsp;</p>
<div class="endscriptcode">The purpose of the view model is to provide the specifics that a particular View needs. My goal in creating the view was to create a &quot;bind-able&quot; surface that would allow the controls in the WPF application to data bind to both data
 and commands.</div>
<div class="endscriptcode"><img src="41727-counterviewmodel.png" alt="" width="302" height="345"></div>
<p>&nbsp;</p>
<p>Binding to the commands causes the buttons on the form to be enabled and disabled as the commands are enabled and disabled. When you bind to data, updates are provided through the PropertyChanged event. However, when you update the state of a command you
 need to use CommandManager.InvalidateRequerySuggested. I found that when I invoked this method on a callback from the workflow the command state was not updated. To solve this problem I created a method to dispatch the call to the UI thread.</p>
<p>&nbsp;</p>
<div class="scriptcode">
<div class="pluginEditHolder" pluginCommand="mceScriptCode">
<div class="title"><span>C#</span></div>
<div class="pluginLinkHolder"><span class="pluginEditHolderLink">Edit</span>|<span class="pluginRemoveHolderLink">Remove</span></div>
<span class="hidden">csharp</span>

<div class="preview">
<pre class="csharp"><span class="cs__keyword">private</span>&nbsp;<span class="cs__keyword">static</span>&nbsp;<span class="cs__keyword">void</span>&nbsp;RequeryCommands()&nbsp;
{&nbsp;
&nbsp;&nbsp;&nbsp;&nbsp;<span class="cs__com">//&nbsp;May&nbsp;be&nbsp;called&nbsp;at&nbsp;shutdown</span>&nbsp;
&nbsp;&nbsp;&nbsp;&nbsp;<span class="cs__keyword">if</span>&nbsp;(Application.Current&nbsp;!=&nbsp;<span class="cs__keyword">null</span>)&nbsp;
&nbsp;&nbsp;&nbsp;&nbsp;{&nbsp;
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;Application.Current.Dispatcher.Invoke(&nbsp;
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;(Action)(()&nbsp;=&gt;&nbsp;CommandManager.InvalidateRequerySuggested()));&nbsp;
&nbsp;&nbsp;&nbsp;&nbsp;}&nbsp;
}&nbsp;
</pre>
</div>
</div>
</div>
<div class="endscriptcode">&nbsp;The rest of the code in the ViewModel is pretty straightforward.</div>
<p>&nbsp;</p>
<h3>Binding to the ViewModel</h3>
<p>DataBinding in XAML can be tricky at first but here is how I did it. In the MainWindow constructor I create the view model, set the DataContext to the view model and setup event handlers to solve the window closing problem.</p>
<p>&nbsp;</p>
<div class="scriptcode">
<div class="pluginEditHolder" pluginCommand="mceScriptCode">
<div class="title"><span>C#</span></div>
<div class="pluginLinkHolder"><span class="pluginEditHolderLink">Edit</span>|<span class="pluginRemoveHolderLink">Remove</span></div>
<span class="hidden">csharp</span>

<div class="preview">
<pre class="csharp"><span class="cs__keyword">public</span>&nbsp;MainWindow()&nbsp;
{&nbsp;
&nbsp;&nbsp;&nbsp;&nbsp;<span class="cs__keyword">this</span>.InitializeComponent();&nbsp;
&nbsp;
&nbsp;&nbsp;&nbsp;&nbsp;<span class="cs__keyword">this</span>.viewModel&nbsp;=&nbsp;<span class="cs__keyword">new</span>&nbsp;CounterViewModel();&nbsp;
&nbsp;&nbsp;&nbsp;&nbsp;<span class="cs__keyword">this</span>.Closing&nbsp;&#43;=&nbsp;<span class="cs__keyword">this</span>.viewModel.ViewClosing;&nbsp;
&nbsp;&nbsp;&nbsp;&nbsp;<span class="cs__keyword">this</span>.Closed&nbsp;&#43;=&nbsp;<span class="cs__keyword">this</span>.viewModel.ViewClosed;&nbsp;
&nbsp;
&nbsp;&nbsp;&nbsp;&nbsp;<span class="cs__keyword">this</span>.DataContext&nbsp;=&nbsp;<span class="cs__keyword">this</span>.viewModel;&nbsp;
}</pre>
</div>
</div>
</div>
<div class="endscriptcode">&nbsp;This is all the code in the code behind for the MainWindow class. It is difficult to test code in the window class so this is a good thing.</div>
<p>&nbsp;</p>
<p>Here is how I did the databinding for the progress bar.</p>
<p>&nbsp;</p>
<div class="scriptcode">
<div class="pluginEditHolder" pluginCommand="mceScriptCode">
<div class="title"><span>XAML</span></div>
<div class="pluginLinkHolder"><span class="pluginEditHolderLink">Edit</span>|<span class="pluginRemoveHolderLink">Remove</span></div>
<span class="hidden">xaml</span>

<div class="preview">
<pre class="xaml"><span class="xaml__tag_start">&lt;ProgressBar</span>&nbsp;&nbsp;
&nbsp;&nbsp;&nbsp;&nbsp;x:<span class="xaml__attr_name">Name</span>=<span class="xaml__attr_value">&quot;progressBar&quot;</span>&nbsp;&nbsp;
&nbsp;&nbsp;&nbsp;&nbsp;Grid.<span class="xaml__attr_name">Column</span>=<span class="xaml__attr_value">&quot;0&quot;</span>&nbsp;&nbsp;
&nbsp;&nbsp;&nbsp;&nbsp;Grid.<span class="xaml__attr_name">Row</span>=<span class="xaml__attr_value">&quot;2&quot;</span>&nbsp;&nbsp;
&nbsp;&nbsp;&nbsp;&nbsp;Grid.<span class="xaml__attr_name">ColumnSpan</span>=<span class="xaml__attr_value">&quot;2&quot;</span>&nbsp;&nbsp;&nbsp;
&nbsp;&nbsp;&nbsp;&nbsp;<span class="xaml__attr_name">Value</span>=<span class="xaml__attr_value">&quot;{Binding&nbsp;Path=CurrentCount,&nbsp;Mode=OneWay}&quot;</span>&nbsp;
&nbsp;&nbsp;&nbsp;&nbsp;<span class="xaml__attr_name">Maximum</span>=<span class="xaml__attr_value">&quot;{Binding&nbsp;Path=CountTo,&nbsp;Mode=OneWay}&quot;</span>&nbsp;
&nbsp;&nbsp;&nbsp;&nbsp;<span class="xaml__tag_start">/&gt;</span>&nbsp;
</pre>
</div>
</div>
</div>
<div class="endscriptcode">Binding to commands is similar</div>
<div class="endscriptcode">
<div class="scriptcode">
<div class="pluginEditHolder" pluginCommand="mceScriptCode">
<div class="title"><span>XAML</span></div>
<div class="pluginLinkHolder"><span class="pluginEditHolderLink">Edit</span>|<span class="pluginRemoveHolderLink">Remove</span></div>
<span class="hidden">xaml</span>

<div class="preview">
<pre class="xaml"><span class="xaml__tag_start">&lt;Button</span>&nbsp;&nbsp;
&nbsp;&nbsp;&nbsp;&nbsp;<span class="xaml__attr_name">FontSize</span>=<span class="xaml__attr_value">&quot;32&quot;</span>&nbsp;&nbsp;
&nbsp;&nbsp;&nbsp;&nbsp;Grid.<span class="xaml__attr_name">Row</span>=<span class="xaml__attr_value">&quot;1&quot;</span>&nbsp;
&nbsp;&nbsp;&nbsp;&nbsp;Grid.<span class="xaml__attr_name">Column</span>=<span class="xaml__attr_value">&quot;0&quot;</span>&nbsp;
&nbsp;&nbsp;&nbsp;&nbsp;<span class="xaml__attr_name">Command</span>=<span class="xaml__attr_value">&quot;{Binding&nbsp;Path=StartCommand}&quot;</span>&nbsp;
&nbsp;&nbsp;&nbsp;&nbsp;<span class="xaml__tag_start">&gt;</span>Start<span class="xaml__tag_end">&lt;/Button&gt;</span>&nbsp;
</pre>
</div>
</div>
</div>
<div class="endscriptcode">&nbsp;</div>
</div>
<p>&nbsp;</p>
<h3>Testing the ViewModel</h3>
<p>Initially the ViewModel didn't include delegates that surface the notification events from the model because I didn't write the tests first and instead created the UI. In retrospect I should have created the tests at the same time as I created the View Model.
 I would have realized that it was impossible to test the View Model without the events because the test code has to know about the events in the workflow.</p>
<p>For example, here is a test where I want to verify the state of the StartCommand</p>
<p>&nbsp;</p>
<div class="scriptcode">
<div class="pluginEditHolder" pluginCommand="mceScriptCode">
<div class="title"><span>C#</span></div>
<div class="pluginLinkHolder"><span class="pluginEditHolderLink">Edit</span>|<span class="pluginRemoveHolderLink">Remove</span></div>
<span class="hidden">csharp</span>

<div class="preview">
<pre class="csharp"><span class="cs__com">///&nbsp;&lt;summary&gt;</span>&nbsp;
<span class="cs__com">///&nbsp;Verifies&nbsp;that&nbsp;the&nbsp;StartCounting&nbsp;command&nbsp;is&nbsp;enabled/disabled&nbsp;as&nbsp;expected</span>&nbsp;
<span class="cs__com">///&lt;/summary&gt;</span>&nbsp;
[TestMethod()]&nbsp;
<span class="cs__keyword">public</span>&nbsp;<span class="cs__keyword">void</span>&nbsp;CanStartCountingTest()&nbsp;
{&nbsp;
&nbsp;&nbsp;&nbsp;&nbsp;CounterViewModel&nbsp;target&nbsp;=&nbsp;<span class="cs__keyword">new</span>&nbsp;CounterViewModel();&nbsp;
&nbsp;&nbsp;&nbsp;&nbsp;AutoResetEvent&nbsp;countStarted&nbsp;=&nbsp;<span class="cs__keyword">new</span>&nbsp;AutoResetEvent(<span class="cs__keyword">false</span>);&nbsp;
&nbsp;&nbsp;&nbsp;&nbsp;AutoResetEvent&nbsp;countCompleted&nbsp;=&nbsp;<span class="cs__keyword">new</span>&nbsp;AutoResetEvent(<span class="cs__keyword">false</span>);&nbsp;
&nbsp;&nbsp;&nbsp;&nbsp;target.CountStarted&nbsp;&#43;=&nbsp;()&nbsp;=&gt;&nbsp;countStarted.Set();&nbsp;
&nbsp;&nbsp;&nbsp;&nbsp;target.CountCompleted&nbsp;&#43;=&nbsp;()&nbsp;=&gt;&nbsp;countCompleted.Set();&nbsp;
&nbsp;
&nbsp;&nbsp;&nbsp;&nbsp;Assert.IsTrue(target.CanStartCounting(),&nbsp;<span class="cs__string">&quot;Start&nbsp;command&nbsp;is&nbsp;not&nbsp;enabled&quot;</span>);&nbsp;
&nbsp;
&nbsp;&nbsp;&nbsp;&nbsp;target.CountTo&nbsp;=&nbsp;<span class="cs__number">5</span>;&nbsp;
&nbsp;&nbsp;&nbsp;&nbsp;target.StartCounting();&nbsp;
&nbsp;
&nbsp;&nbsp;&nbsp;&nbsp;Assert.IsTrue(countStarted.WaitOne(<span class="cs__number">1000</span>));&nbsp;
&nbsp;
&nbsp;&nbsp;&nbsp;&nbsp;Assert.IsFalse(target.CanStartCounting(),&nbsp;<span class="cs__string">&quot;Start&nbsp;command&nbsp;is&nbsp;not&nbsp;disabled&quot;</span>);&nbsp;
&nbsp;
&nbsp;&nbsp;&nbsp;&nbsp;Assert.IsTrue(countCompleted.WaitOne(<span class="cs__number">1000</span>));&nbsp;
&nbsp;
&nbsp;&nbsp;&nbsp;&nbsp;Assert.IsTrue(target.CanStartCounting(),&nbsp;<span class="cs__string">&quot;Start&nbsp;command&nbsp;is&nbsp;not&nbsp;enabled&quot;</span>);&nbsp;
}&nbsp;
</pre>
</div>
</div>
</div>
<h1 class="endscriptcode">Summary</h1>
<p>&nbsp;</p>
<p>This sample application demonstrates that it is possible to build a WPF app using the MVVM pattern that interacts with a workflow. Using a Workflow to do processing makes your app more responsive because the workflow is running on a different thread. Also,
 though this app does not show it, you could run many workflows all at the same time by interacting with a collection of them rather than a single one like this application does.</p>
