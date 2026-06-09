#!/usr/bin/env python3
# run.py - Lightweight server and browser launcher for Aether Lancer: Cyber Clash
# This bypasses CORS issues with ES module loading over file:// protocol.

import http.server
import socketserver
import webbrowser
import threading
import time
import sys

PORT = 8000

class QuietHTTPRequestHandler(http.server.SimpleHTTPRequestHandler):
    def log_message(self, format, *args):
        # Suppress logging request details to keep the console clean
        return

def start_server():
    # Allow port reuse to prevent address-already-in-use errors
    socketserver.TCPServer.allow_reuse_address = True
    try:
        with socketserver.TCPServer(("", PORT), QuietHTTPRequestHandler) as httpd:
            httpd.serve_forever()
    except Exception as e:
        print(f"\n[ERROR] Failed to start server: {e}", file=sys.stderr)
        sys.exit(1)

if __name__ == "__main__":
    print("=" * 60)
    print("  🚀 AETHER LANCER: CYBER CLASH - Local Session Launcher")
    print("=" * 60)
    print(f"Connecting to neural network at port {PORT}...")

    # Start the server in a daemon thread so it exits when the main thread stops
    server_thread = threading.Thread(target=start_server, daemon=True)
    server_thread.start()

    # Wait a moment for server to bind
    time.sleep(0.5)

    url = f"http://localhost:{PORT}"
    print(f"Decrypting data streams: {url}")
    print("Opening default browser...")
    
    # Launch browser
    webbrowser.open(url)
    
    print("\n[SUCCESS] Link established. Press Ctrl+C to terminate connection.")
    print("=" * 60)

    try:
        while True:
            time.sleep(1)
    except KeyboardInterrupt:
        print("\nConnection terminated. Safely shutting down server.")
        sys.exit(0)
