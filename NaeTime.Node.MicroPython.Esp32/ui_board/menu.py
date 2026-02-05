# menu.py
# Menu class for TFT display with D-pad navigation

import ST7735
import font5x8


class MenuItem:
    """Represents a single menu item with a label, optional callback, or submenu."""
    
    def __init__(self, label, callback=None, submenu=None):
        self.label = label
        self.callback = callback
        self.submenu = submenu  # Reference to another Menu instance


class Menu:
    """
    A menu that can be displayed on a TFT screen and navigated with a D-pad.
    
    Usage:
        menu = Menu(tft, dpad)
        menu.add_item("Option 1", callback1)
        menu.add_item("Option 2", callback2)
        menu.add_item("Back", None)
        menu.show()
        
        # In main loop:
        menu.update()
    """
    
    def __init__(self, tft, dpad, title="Menu", 
                 x=5, y=20, width=150, item_height=12,
                 bg_color=None, text_color=None, 
                 selected_bg=None, selected_text=None,
                 parent_menu=None):
        """
        Initialize the menu.
        
        Args:
            tft: ST7735.TFT instance
            dpad: DPad instance
            title: Menu title text
            x, y: Top-left position of menu
            width: Width of menu area
            item_height: Height of each menu item in pixels
            bg_color: Background color (default: BLACK)
            text_color: Text color (default: WHITE)
            selected_bg: Selected item background color (default: BLUE)
            selected_text: Selected item text color (default: YELLOW)
            parent_menu: Parent menu for back navigation (default: None)
        """
        self.tft = tft
        self.dpad = dpad
        self.title = title
        self.x = x
        self.y = y
        self.width = width
        self.item_height = item_height
        
        # Colors
        self.bg_color = bg_color if bg_color is not None else ST7735.TFT.BLACK
        self.text_color = text_color if text_color is not None else ST7735.TFT.WHITE
        self.selected_bg = selected_bg if selected_bg is not None else ST7735.TFT.ORANGE
        self.selected_text = selected_text if selected_text is not None else ST7735.TFT.WHITE
        
        # Menu items
        self.items = []
        self.selected_index = 0
        self.visible = False
        
        # Submenu support
        self.parent_menu = parent_menu
        self.active_submenu = None
        
        # Scrolling support
        self.scroll_offset = 0
        self.max_visible_items = 8  # Adjust based on screen size
        
        # Navigation state tracking
        self._last_dpad_state = None
        self._debounce_time = 200  # ms between navigation inputs
        self._last_action_time = 0
        
    def add_item(self, label, callback=None, submenu=None):
        """Add a menu item with optional callback or submenu."""
        self.items.append(MenuItem(label, callback, submenu))
    
    def add_submenu(self, label, submenu):
        """Add a submenu item. The submenu's parent will be set to this menu."""
        submenu.parent_menu = self
        self.items.append(MenuItem(label, submenu=submenu))
        
    def clear_items(self):
        """Remove all menu items."""
        self.items.clear()
        self.selected_index = 0
        self.scroll_offset = 0
        self.active_submenu = None
        
    def show(self):
        """Display the menu on the screen."""
        self.visible = True
        self._draw_full_menu()
        
    def hide(self):
        """Hide the menu (clear the screen)."""
        self.visible = False
        self.tft.fill(self.bg_color)
        
    def _draw_full_menu(self):
        """Draw the entire menu including title and all visible items."""
        # Clear screen
        self.tft.fill(self.bg_color)
        
        # Draw title
        title_y = self.y - 15
        self.tft.text((self.x, title_y), self.title, self.text_color, font5x8.font5x8)
        
        # Draw separator line (simple dots)
        separator = "-" * (self.width // 6)
        self.tft.text((self.x, title_y + 10), separator, self.text_color, font5x8.font5x8)
        
        # Draw menu items
        for i in range(len(self.items)):
            self._draw_item(i, i == self.selected_index)
    
    def _draw_item(self, index, is_selected):
        """Draw a single menu item."""
        if index < self.scroll_offset:
            return
        if index >= self.scroll_offset + self.max_visible_items:
            return
            
        # Calculate position
        visible_index = index - self.scroll_offset
        item_y = self.y + (visible_index * self.item_height)
        
        # Choose colors based on selection
        if is_selected:
            bg = self.selected_bg
            fg = self.selected_text
            prefix = ">"
        else:
            bg = self.bg_color
            fg = self.text_color
            prefix = " "
        
        # Draw background for entire line
        self.tft.fillrect((self.x, item_y), (self.width, self.item_height), bg)
        
        # Draw text with selection indicator and background color
        label = f"{prefix} {self.items[index].label}"
        self.tft.text((self.x + 2, item_y + 2), label, fg, font5x8.font5x8, bgColor=bg)
    
    def _update_selection(self, old_index, new_index):
        """Update the display when selection changes."""
        # Redraw old selected item as unselected
        self._draw_item(old_index, False)
        
        # Redraw new selected item as selected
        self._draw_item(new_index, True)
    
    def _handle_scroll(self):
        """Adjust scroll offset to keep selected item visible."""
        # Scroll down if selected item is below visible area
        if self.selected_index >= self.scroll_offset + self.max_visible_items:
            self.scroll_offset = self.selected_index - self.max_visible_items + 1
            self._draw_full_menu()
        
        # Scroll up if selected item is above visible area
        elif self.selected_index < self.scroll_offset:
            self.scroll_offset = self.selected_index
            self._draw_full_menu()
    
    def update(self):
        """
        Update menu state based on D-pad input.
        Call this in your main loop.
        
        Returns:
            True if an item was selected (enter pressed), False otherwise
        """
        # If a submenu is active, delegate to it
        if self.active_submenu is not None:
            return self.active_submenu.update()
        
        if not self.visible or len(self.items) == 0:
            return False
        
        import time
        current_time = time.ticks_ms()
        
        # Read D-pad state
        dpad_state = self.dpad.read()
        
        # Check for up navigation
        if dpad_state["up"]["state"] == "button_down":
            if time.ticks_diff(current_time, self._last_action_time) > self._debounce_time:
                old_index = self.selected_index
                self.selected_index = max(0, self.selected_index - 1)
                if old_index != self.selected_index:
                    self._update_selection(old_index, self.selected_index)
                    self._handle_scroll()
                self._last_action_time = current_time
        
        # Check for down navigation
        elif dpad_state["down"]["state"] == "button_down":
            if time.ticks_diff(current_time, self._last_action_time) > self._debounce_time:
                old_index = self.selected_index
                self.selected_index = min(len(self.items) - 1, self.selected_index + 1)
                if old_index != self.selected_index:
                    self._update_selection(old_index, self.selected_index)
                    self._handle_scroll()
                self._last_action_time = current_time
        
        # Check for select action (right button)
        elif dpad_state["right"]["state"] == "button_down":
            if time.ticks_diff(current_time, self._last_action_time) > self._debounce_time:
                self._select_item()
                self._last_action_time = current_time
                return True
        
        # Check for back action (left button)
        elif dpad_state["left"]["state"] == "button_down":
            if time.ticks_diff(current_time, self._last_action_time) > self._debounce_time:
                if self.parent_menu is not None:
                    self.go_back()
                self._last_action_time = current_time
        
        return False
    
    def _select_item(self):
        """Execute the callback or open submenu for the currently selected item."""
        if 0 <= self.selected_index < len(self.items):
            item = self.items[self.selected_index]
            
            # If item has a submenu, open it
            if item.submenu is not None:
                self.open_submenu(item.submenu)
            # Otherwise execute callback
            elif item.callback is not None:
                item.callback()
    
    def get_selected_index(self):
        """Return the currently selected item index."""
        return self.selected_index
    
    def get_selected_item(self):
        """Return the currently selected MenuItem."""
        if 0 <= self.selected_index < len(self.items):
            return self.items[self.selected_index]
        return None
    
    def set_selected_index(self, index):
        """Set the selected item by index."""
        if 0 <= index < len(self.items):
            old_index = self.selected_index
            self.selected_index = index
            if self.visible:
                self._update_selection(old_index, self.selected_index)
                self._handle_scroll()
    
    def open_submenu(self, submenu):
        """Open a submenu."""
        self.active_submenu = submenu
        submenu.parent_menu = self
        submenu.show()
    
    def go_back(self):
        """Go back to parent menu."""
        if self.parent_menu is not None:
            self.hide()
            self.parent_menu.active_submenu = None
            self.parent_menu.show()
    
    def add_back_item(self, label="< Back"):
        """Convenience method to add a back item that returns to parent menu."""
        if self.parent_menu is not None:
            self.add_item(label, lambda: self.go_back())
