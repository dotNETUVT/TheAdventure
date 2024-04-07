using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Runtime.InteropServices;
using Silk.NET.Core.Contexts;

namespace TheAdventure;

/// @class SdlContext
/// @brief Manages the loading and access of native SDL library functions.
///
/// `SdlContext` is responsible for loading the SDL library dynamically at runtime and providing access
/// to its functions. It supports multiple platforms by loading the corresponding native library according to the
/// running OS and architecture. This class implements `INativeContext` to be used with Silk.NET for direct
/// SDL interop calls.
public class SdlContext : INativeContext
{
    /// Handle to the loaded native SDL library.
    private readonly IntPtr _nativeLibrary;

    /// @brief Constructs an `SdlContext` and loads the SDL library.
    ///
    /// The constructor identifies the running OS and architecture to load the appropriate version of the SDL library.
    /// It supports Linux, macOS, and Windows. ARM64 is supported except on Linux due to compatibility issues.
    /// 
    /// @exception PlatformNotSupportedException Thrown when the operating system or architecture is not supported.
    public SdlContext()
    {
        string runtimesPath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!, "runtimes");

        string libraryName;
        string platform;
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            libraryName = "libSDL2-2.0.so";
            platform = "linux";
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            libraryName = "SDL2.dll";
            platform = "win";
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            _nativeLibrary = NativeLibrary.Load(Path.Combine(runtimesPath, "osx", "native", "libSDL2-2.0.dylib"));
            return;
        }
        else
        {
            throw new PlatformNotSupportedException("Only Linux, macOS, and Windows are supported.");
        }

        if (RuntimeInformation.OSArchitecture == Architecture.X64)
        {
            _nativeLibrary = NativeLibrary.Load(Path.Combine(runtimesPath, platform + "-x64", "native", libraryName));
        }
        else if (RuntimeInformation.OSArchitecture == Architecture.X86)
        {
            _nativeLibrary = NativeLibrary.Load(Path.Combine(runtimesPath, platform + "-x86", "native", libraryName));
        }
        else if (RuntimeInformation.OSArchitecture == Architecture.Arm64)
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                throw new PlatformNotSupportedException("ARM64 is not supported on Linux.");
            }

            _nativeLibrary = NativeLibrary.Load(Path.Combine(runtimesPath, platform + "-arm", "native", libraryName));
        }
        else
        {
            throw new PlatformNotSupportedException("Only x64, x86, and ARM64 are supported.");
        }
    }

    /// @brief Retrieves the address of a function exported by the SDL library.
    ///
    /// @param proc The name of the SDL function to retrieve.
    /// @param slot Optional slot number, not used in this context.
    /// @return The address of the specified function, or IntPtr.Zero if not found.
    public IntPtr GetProcAddress(string proc, int? slot = null)
    {
        return NativeLibrary.GetExport(_nativeLibrary, proc);
    }
    
    /// @brief Tries to retrieve the address of a function exported by the SDL library.
    ///
    /// @param proc The name of the SDL function to retrieve.
    /// @param addr Out parameter that receives the address of the function.
    /// @param slot Optional slot number, not used in this context.
    /// @return True if the function was found, otherwise false.
    public bool TryGetProcAddress(string proc, [UnscopedRef] out IntPtr addr, int? slot = null)
    {
        try
        {
            addr = NativeLibrary.GetExport(_nativeLibrary, proc);
        }
        catch (EntryPointNotFoundException)
        {
            addr = IntPtr.Zero;
        }

        return addr != IntPtr.Zero;
    }

    /// @brief Releases unmanaged resources, specifically the loaded SDL library.
    private void ReleaseUnmanagedResources()
    {
        NativeLibrary.Free(_nativeLibrary);
    }

    /// @brief Disposes of `SdlContext`, freeing the loaded SDL library.
    public void Dispose()
    {
        ReleaseUnmanagedResources();
        GC.SuppressFinalize(this);
    }

    /// @brief Finalizer for `SdlContext` that ensures unmanaged resources are freed.
    ~SdlContext()
    {
        ReleaseUnmanagedResources();
    }
}

