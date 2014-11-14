﻿using UnityEngine;
using System.Collections;
using UnityEditor;
using UnityEditor.Callbacks;
using System.IO;
using System.Collections.Generic;

/// <summary>
/// Post build player.
/// 
/// This script automatically add Framework and Build Options to xcode after Unity successful build project
/// This feature is only for IOS 
/// </summary>
public static class PostBuildPlayer 
{

	// Frameworks Ids  -  These ids have been generated by creating a project using Xcode then
	// extracting the values from the generated project.pbxproj.  The format of this
	// file is not documented by Apple so the correct algorithm for generating these
	// ids is unknown
	//const string CORETELEPHONY_ID = "919BD0C3159C677000C931BE" ;
	//const string CORETELEPHONY_FILEREFID = "919BD0C2159C677000C931BE" ;
	
	// List of all the frameworks to be added to the project
	public struct framework
	{
		public string sName ;
		public string sId ;
		public string sFileId ;
		
		public framework(string name, string myId, string fileid)
		{
			sName = name ;
			sId = myId ;
			sFileId = fileid ;
		}
	}
	
	
	/// Processbuild Function
	[PostProcessBuild] // <- this is where the magic happens
	public static void OnPostProcessBuild(BuildTarget target, string path)
	{
		// 1: Check this is an iOS build before running
		#if UNITY_IPHONE
		{
			// 2: We init our tab and process our project
			//framework[] myFrameworks = { new framework("CoreTelephony.framework", CORETELEPHONY_ID, CORETELEPHONY_FILEREFID) } ;

			//define frames that will be added to build project
			//second param is framework id, third param is framework file ref id
			//to find out id you need to open xcode and add required framework then
			//open xcode project file(***.xcodeproj) with any text editor and search for framework keyword(framework name)
			//second param framework id can be found in "PBXFrameworksBuildPhase" section.
			//third param framework id can be found in "PBXGroup" section
			framework[] addFrameworks = {
				new framework("StoreKit.framework", "4610CAD11A15ED3E0023B1A4", "4610CAD01A15ED3E0023B1A4"),
				new framework("AdSupport.framework", "BE2C4029A78B829DF8577D4A", "B381449E98A8C36657553C5A"),
				new framework("CoreTelephony.framework", "919BD0C3159C677000C931BE", "919BD0C2159C677000C931BE"),
				new framework("EventKit.framework", "4610CAD41A15ED470023B1A4", "4610CAD21A15ED470023B1A4"),
				new framework("EventKitUI.framework", "4610CAD51A15ED470023B1A4", "4610CAD31A15ED470023B1A4"),
				new framework("MessageUI.framework", "617C4608B7B5601D0142AD3A", "D147404FB62FE77A3D734746")
			};


			//log build project path
			//Debug.Log(EditorUserBuildSettings.GetBuildLocation(EditorUserBuildSettings.activeBuildTarget));

			/*
			xcodeprojPath = xcodeprojPath.Substring(0, xcodeprojPath.Length - 16) ;
			string device = "iOSandIpad" ;
			if (PlayerSettings.iOS.targetDevice == iOSTargetDevice.iPadOnly)
				device = "iPad" ;
			else if (PlayerSettings.iOS.targetDevice == iOSTargetDevice.iPhoneOnly)
				device = "iPhone" ;
			xcodeprojPath = xcodeprojPath + "buildios/"+device+"/Unity-iPhone.xcodeproj" ;
			*/

			//          Debug.Log("We found xcodeprojPath to be : "+xcodeprojPath) ;
			
			//Debug.Log("OnPostProcessBuild - START on: "+device) ;


			//updateXcodeProject(xcodeprojPath, myFrameworks) ;
			//updateXcodeProject(xcodeprojPath, addFrameworks) ;

			//find out build project path
			string bProjPath = XcodeBuildProjectPath();

			//start our xcode project modification
			updateXcodeProjectV2(bProjPath, FilterFramework(bProjPath, addFrameworks));

		}
		#else
		// 3: We do nothing if not iPhone
		Debug.Log("OnPostProcessBuild - Warning: This is not an iOS build") ;
		#endif     
		Debug.Log("OnPostProcessBuild - STOP") ;
	}

	// MAIN FUNCTION Version 2 
	// xcodeproj_filename - filename of the Xcode project to change
	// frameworks - list of Apple standard frameworks to add to the project
	public static void updateXcodeProjectV2(string xcodeprojPath, List<framework> listeFrameworks)
	{
		// STEP 1 : We open up the file generated by Unity and read into memory as
		// a list of lines for processing

		string[] lines = System.IO.File.ReadAllLines(xcodeprojPath);

		
		// STEP 2 : We check if file has already been processed and only proceed if it hasn't,
		// we'll do this by looping through the build files and see if CoreTelephony.framework
		// is there
		int i = 0 ;
		
		// STEP 3 : We'll open/replace project.pbxproj for writing and iterate over the old
		// file in memory, copying the original file and inserting every extra we need
		FileStream filestr = new FileStream(xcodeprojPath, FileMode.Create); //Create new file and open it for read and write, if the file exists overwrite it.
		filestr.Close() ;
		StreamWriter fCurrentXcodeProjFile = new StreamWriter(xcodeprojPath) ; // will be used for writing
		
		// As we iterate through the list we'll record which section of the
		// project.pbxproj we are currently in
		string section = "" ;
		
		// We use this boolean to decide whether we have already added the list of
		// build files to the link line.  This is needed because there could be multiple
		// build targets and they are not named in the project.pbxproj
		bool bFrameworks_build_added = false ;
		int iNbBuildConfigSet = 0 ; // can't be > 2


		foreach (string line in lines)
		{
			if (line.StartsWith("\t\t\t\tGCC_ENABLE_CPP_EXCEPTIONS") ||
			    line.StartsWith("\t\t\t\tGCC_ENABLE_CPP_RTTI") ||
			    line.StartsWith("\t\t\t\tGCC_ENABLE_OBJC_EXCEPTIONS") )
			{
				// apparently, we don't copy those lines in our new project
			}
			else
			{                          
				//////////////////////////////
				//  STEP 1 : Build Options  //
				//////////////////////////////
				//
				// TapJoy needs "Enable Object-C Exceptions" to be set to "YES"
				//
				//////////////////////////////
				
				// This one is special, we have to replace a line and not write after
				//          if ( section == "XCBuildConfiguration"  line.Trim().StartsWith("\t\t\t\tGCC_ENABLE_OBJC_EXCEPTIONS") )
				//              fCurrentXcodeProjFile.Write("\t\t\t\tGCC_ENABLE_OBJC_EXCEPTIONS = YES;\n") ;
				
				// in any other situation, we'll first copy the line in our new project, then we might do something special regarding that line
				//              else
				fCurrentXcodeProjFile.WriteLine(line) ;
				
				
				
				//////////////////////////////////
				//  STEP 2 : Include Framewoks  //
				//////////////////////////////////
				//
				// TapJoy needs CoreTelephony (in weak-link = "optional")
				//
				//////////////////////////////////
				
				// Each section starts with a comment such as : /* Begin PBXBuildFile section */'
				if ( (lines[i].Length > 7) &&  (string.Compare(lines[i].Substring(3, 5), "Begin") == 0  ))
				{
					section = line.Split(' ')[2] ;
					//Debug.Log("NEW_SECTION: "+section) ;
					
					if (section == "PBXBuildFile")
					{
						foreach (framework fr in listeFrameworks)
							add_build_file(fCurrentXcodeProjFile, fr.sId, fr.sName, fr.sFileId) ;
					}
					
					if (section == "PBXFileReference")
					{
						foreach (framework fr in listeFrameworks)
							add_framework_file_reference(fCurrentXcodeProjFile, fr.sFileId, fr.sName) ;
					}
					
					
					if ((line.Length > 5) &&  (string.Compare(line.Substring(3, 3), "End") == 0))
						section = "" ;
				}
				// The PBXResourcesBuildPhase section is what appears in XCode as 'Link
				// Binary With Libraries'.  As with the frameworks we make the assumption the
				// first target is always 'Unity-iPhone' as the name of the target itself is
				// not listed in project.pbxproj   
				
				if ((section == "PBXFrameworksBuildPhase") &&
				    (line.Trim().Length > 4) &&
				    ( string.Compare(line.Trim().Substring(0, 5) , "files") == 0) &&
				    (!bFrameworks_build_added))
				{
					foreach (framework fr in listeFrameworks)
						add_frameworks_build_phase(fCurrentXcodeProjFile, fr.sId, fr.sName) ;
					bFrameworks_build_added = true ;
				}
				
				
				// The PBXGroup is the section that appears in XCode as 'Copy Bundle Resources'.
				
				if ((section == "PBXGroup") &&
				    (line.Trim().Length > 7 ) &&
				    (string.Compare(line.Trim().Substring(0, 8) , "children") == 0) &&
				    (lines[i-2].Trim().Split(' ').Length > 0) &&
				    (string.Compare(lines[i-2].Trim().Split(' ')[2] , "CustomTemplate" ) == 0) )
				{
					foreach (framework fr in listeFrameworks)
						add_group(fCurrentXcodeProjFile, fr.sFileId, fr.sName) ;
				}
				
				
				//////////////////////////////
				//  STEP 3 : Build Options  //
				//////////////////////////////
				//
				// AdColony needs "Other Linker Flags" to have "-all_load -ObjC" added to its value
				//
				//////////////////////////////
				if ((section == "XCBuildConfiguration") &&
				    (line.StartsWith("\t\t\t\tOTHER_LDFLAGS")) &&
				    (iNbBuildConfigSet < 2))
				{
					//fCurrentXcodeProjFile.Write("\t\t\t\t\t\"-all_load\",\n") ;
					fCurrentXcodeProjFile.Write("\t\t\t\t\t\"-ObjC\",\n") ;
					Debug.Log("OnPostProcessBuild - Adding \"-ObjC\" flag to build options") ; // \"-all_load\" and
					++iNbBuildConfigSet ;
				}
			}
			++i ;
		}

		fCurrentXcodeProjFile.Close() ;
	}
	
	
//	// MAIN FUNCTION
//	// xcodeproj_filename - filename of the Xcode project to change
//	// frameworks - list of Apple standard frameworks to add to the project
//	public static void updateXcodeProject(string xcodeprojPath, framework[] listeFrameworks)
//	{
//		// STEP 1 : We open up the file generated by Unity and read into memory as
//		// a list of lines for processing
//		string project = xcodeprojPath + "/project.pbxproj" ;
//		Debug.Log ("OnPostProcessBuild - Reading file at " + project);
//		string[] lines = System.IO.File.ReadAllLines(project);
//		
//		// STEP 2 : We check if file has already been processed and only proceed if it hasn't,
//		// we'll do this by looping through the build files and see if CoreTelephony.framework
//		// is there
//		int i = 0 ;
//
//		bool bFound = false ;
//		bool bEnd = false ;
//
//		/*
//		while ( !bFound && !bEnd)
//		{
//			if ((lines[i].Length > 5) && (string.Compare(lines[i].Substring(3, 3), "End") == 0) )
//				bEnd = true ;
//			
//			bFound = lines[i].Contains("CoreTelephony.framework") ;
//			++i ;
//		}
//		*/
//
//		if (bFound)
//			Debug.Log("OnPostProcessBuild - ERROR: Frameworks have already been added to XCode project") ;
//		else
//		{
//			// STEP 3 : We'll open/replace project.pbxproj for writing and iterate over the old
//			// file in memory, copying the original file and inserting every extra we need
//			FileStream filestr = new FileStream(project, FileMode.Create); //Create new file and open it for read and write, if the file exists overwrite it.
//			filestr.Close() ;
//			StreamWriter fCurrentXcodeProjFile = new StreamWriter(project) ; // will be used for writing
//			
//			// As we iterate through the list we'll record which section of the
//			// project.pbxproj we are currently in
//			string section = "" ;
//			
//			// We use this boolean to decide whether we have already added the list of
//			// build files to the link line.  This is needed because there could be multiple
//			// build targets and they are not named in the project.pbxproj
//			bool bFrameworks_build_added = false ;
//			int iNbBuildConfigSet = 0 ; // can't be > 2
//			
//			i = 0 ;
//			foreach (string line in lines)
//			{
//				if (line.StartsWith("\t\t\t\tGCC_ENABLE_CPP_EXCEPTIONS") ||
//				    line.StartsWith("\t\t\t\tGCC_ENABLE_CPP_RTTI") ||
//				    line.StartsWith("\t\t\t\tGCC_ENABLE_OBJC_EXCEPTIONS") )
//				{
//					// apparently, we don't copy those lines in our new project
//				}
//				else
//				{                          
//					//////////////////////////////
//					//  STEP 1 : Build Options  //
//					//////////////////////////////
//					//
//					// TapJoy needs "Enable Object-C Exceptions" to be set to "YES"
//					//
//					//////////////////////////////
//					
//					// This one is special, we have to replace a line and not write after
//					//          if ( section == "XCBuildConfiguration"  line.Trim().StartsWith("\t\t\t\tGCC_ENABLE_OBJC_EXCEPTIONS") )
//					//              fCurrentXcodeProjFile.Write("\t\t\t\tGCC_ENABLE_OBJC_EXCEPTIONS = YES;\n") ;
//					
//					// in any other situation, we'll first copy the line in our new project, then we might do something special regarding that line
//					//              else
//					fCurrentXcodeProjFile.WriteLine(line) ;
//					
//					
//					
//					//////////////////////////////////
//					//  STEP 2 : Include Framewoks  //
//					//////////////////////////////////
//					//
//					// TapJoy needs CoreTelephony (in weak-link = "optional")
//					//
//					//////////////////////////////////
//					
//					// Each section starts with a comment such as : /* Begin PBXBuildFile section */'
//					if ( (lines[i].Length > 7) &&  (string.Compare(lines[i].Substring(3, 5), "Begin") == 0  ))
//					{
//						section = line.Split(' ')[2] ;
//						//Debug.Log("NEW_SECTION: "+section) ;
//
//						if (section == "PBXBuildFile")
//						{
//							foreach (framework fr in listeFrameworks)
//								add_build_file(fCurrentXcodeProjFile, fr.sId, fr.sName, fr.sFileId) ;
//						}
//						
//						if (section == "PBXFileReference")
//						{
//							foreach (framework fr in listeFrameworks)
//								add_framework_file_reference(fCurrentXcodeProjFile, fr.sFileId, fr.sName) ;
//						}
//
//						
//						if ((line.Length > 5) &&  (string.Compare(line.Substring(3, 3), "End") == 0))
//							section = "" ;
//					}
//					// The PBXResourcesBuildPhase section is what appears in XCode as 'Link
//					// Binary With Libraries'.  As with the frameworks we make the assumption the
//					// first target is always 'Unity-iPhone' as the name of the target itself is
//					// not listed in project.pbxproj   
//
//					if ((section == "PBXFrameworksBuildPhase") &&
//					    (line.Trim().Length > 4) &&
//					   ( string.Compare(line.Trim().Substring(0, 5) , "files") == 0) &&
//					    (!bFrameworks_build_added))
//					{
//						foreach (framework fr in listeFrameworks)
//							add_frameworks_build_phase(fCurrentXcodeProjFile, fr.sId, fr.sName) ;
//						bFrameworks_build_added = true ;
//					}
//
//					
//					// The PBXGroup is the section that appears in XCode as 'Copy Bundle Resources'.
//
//					if ((section == "PBXGroup") &&
//					    (line.Trim().Length > 7 ) &&
//					    (string.Compare(line.Trim().Substring(0, 8) , "children") == 0) &&
//					    (lines[i-2].Trim().Split(' ').Length > 0) &&
//					    (string.Compare(lines[i-2].Trim().Split(' ')[2] , "CustomTemplate" ) == 0) )
//					{
//						foreach (framework fr in listeFrameworks)
//							add_group(fCurrentXcodeProjFile, fr.sFileId, fr.sName) ;
//					}
//
//
//					//////////////////////////////
//					//  STEP 3 : Build Options  //
//					//////////////////////////////
//					//
//					// AdColony needs "Other Linker Flags" to have "-all_load -ObjC" added to its value
//					//
//					//////////////////////////////
//					if ((section == "XCBuildConfiguration") &&
//					    (line.StartsWith("\t\t\t\tOTHER_LDFLAGS")) &&
//					    (iNbBuildConfigSet < 2))
//					{
//						//fCurrentXcodeProjFile.Write("\t\t\t\t\t\"-all_load\",\n") ;
//						fCurrentXcodeProjFile.Write("\t\t\t\t\t\"-ObjC\",\n") ;
//						Debug.Log("OnPostProcessBuild - Adding \"-ObjC\" flag to build options") ; // \"-all_load\" and
//						++iNbBuildConfigSet ;
//					}
//				}
//				++i ;
//			}
//			fCurrentXcodeProjFile.Close() ;
//		}
//	}
	
	
	/////////////////
	///////////
	// ROUTINES
	///////////
	/////////////////

	//Return Unity build project's path and modify path for xcode(project.pbxproj)
	private static string XcodeBuildProjectPath()
	{
		//get build project path
		string xcodeprojPath = EditorUserBuildSettings.GetBuildLocation(EditorUserBuildSettings.activeBuildTarget);

		//add standard fiel path
		xcodeprojPath = xcodeprojPath + "/Unity-iPhone.xcodeproj/project.pbxproj";

		Debug.Log("OnPostProcessBuild...project dir:"+xcodeprojPath);

		return xcodeprojPath;
	}

	//Determine whether framework should add to build xcode project or not
	//Check if framework is already in xcode project and remove from framework list which is going to add to
	private static List<framework> FilterFramework(string xcodeprojPath, framework[] listFramework)
	{
		//final framework that will be added to build project
		List<framework> filterFramework = new List<framework> ();

		//framework that will not be added to build project
		HashSet<framework> removedFramework = new HashSet<framework> ();

		//add defined framework to list
		for(int i=0; i<listFramework.Length; i++)
		{
			filterFramework.Add(listFramework[i]);
		}

		//return framework if there is non
		if(filterFramework.Count == 0)
		{
			return filterFramework;
		}

		//read xocde project all line in file
		string[] lines = System.IO.File.ReadAllLines(xcodeprojPath);

		//determine framework should be add to project or not
		//by run through each line
		int j = 0 ;
		foreach (string line in lines)
		{
			for(int o=0; o<filterFramework.Count; o++)
			{
				if(line.Contains(filterFramework[o].sName))
				{
					if(!removedFramework.Contains(filterFramework[o]))
					{
						Debug.LogWarning("OnPostProcessBuild - Framework:"+filterFramework[o].sName+" already exist and will not add to build project");
					}

					removedFramework.Add(filterFramework[o]);


				}
			}

			++j;
		}

		//eliminate framework that should not be added 
		foreach(framework f in removedFramework)
		{
			filterFramework.Remove(f);
		}

		//return framework
		return filterFramework;
	}

	// Adds a line into the PBXBuildFile section
	private static void add_build_file(StreamWriter file, string id, string name, string fileref)
	{
		Debug.Log("OnPostProcessBuild - Adding build file " + name) ;
		string subsection = "Frameworks" ;
		
		if (name == "CoreTelephony.framework")  // CoreTelephony.framework should be weak-linked
			file.Write("\t\t"+id+" /* "+name+" in "+subsection+" */ = {isa = PBXBuildFile; fileRef = "+fileref+" /* "+name+" */; settings = {ATTRIBUTES = (Weak, ); }; };") ;
		else // Others framework are normal
			file.Write("\t\t"+id+" /* "+name+" in "+subsection+" */ = {isa = PBXBuildFile; fileRef = "+fileref+" /* "+name+" */; };\n") ;
	}
	
	// Adds a line into the PBXBuildFile section
	private static void add_framework_file_reference(StreamWriter file, string id, string name)
	{
		Debug.Log("OnPostProcessBuild - Adding framework file reference " + name) ;
		
		string path = "System/Library/Frameworks" ; // all the frameworks come from here
		if (name == "libsqlite3.0.dylib")           // except for lidsqlite
			path = "usr/lib" ;
		
		file.Write("\t\t"+id+" /* "+name+" */ = {isa = PBXFileReference; lastKnownFileType = wrapper.framework; name = "+name+"; path = "+path+"/"+name+"; sourceTree = SDKROOT; };\n") ;
	}
	
	// Adds a line into the PBXFrameworksBuildPhase section
	private static void add_frameworks_build_phase(StreamWriter file, string id, string name)
	{
		Debug.Log("OnPostProcessBuild - Adding build phase " + name) ;
		
		file.Write("\t\t\t\t"+id+" /* "+name+" in Frameworks */,\n") ;
	}
	
	// Adds a line into the PBXGroup section
	private static void add_group(StreamWriter file, string id, string name)
	{
		Debug.Log("OnPostProcessBuild - Add group " + name) ;
		
		file.Write("\t\t\t\t"+id+" /* "+name+" */,\n") ;
	}
}
