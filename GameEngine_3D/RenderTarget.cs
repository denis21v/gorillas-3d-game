////////////////////////////////////////////////////////////////////////////////
//                                                                            //
//      Submitted for the BSc in Computer Science for Games Development       //
//      Project code: SG7 (3D Gorillas)                                       //
//      By Denis Volosin                                                      //
//                                                                            //
//      RenderTarget.cs                                                       //
//                                                                            //
//      Abstract render target object                                         //
//                                                                            //
////////////////////////////////////////////////////////////////////////////////

using System;
using OpenTK.Input;

namespace GameEngine_3D
{
    ///////////////////////////////////////////////////////////////////////////
    // IRenderTarget interface

    public interface IRenderTarget
    {
        ///////////////////////////////////////////////////////////////////////
        // Event handlers to be implemented by render target interface
        // conforming classes

        // Engine load handler
        void OnLoad();

        // Window resize handler
        void OnResize();

        // Frame update handler
        void OnUpdate();

        // Frame render handler
        void OnRender();

        // User input handlers. Methods below are expected to return
        // true if the input event has been handled by render target
        // and false otherwise.
        bool OnMouseDown(MouseButtonEventArgs args);
        bool OnMouseMove(MouseMoveEventArgs args);
        bool OnMouseUp(MouseButtonEventArgs args);
        bool OnMouseWheel(MouseWheelEventArgs args);
        bool OnKeyDown(KeyboardKeyEventArgs args);
        bool OnKeyUp(KeyboardKeyEventArgs args);
    }
}
