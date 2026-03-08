# menu.py
# Menu class for TFT display with D-pad navigation

import ST7735
import font5x8


class MenuItem:
    """
    Represents a single menu item.

    Action items:  provide callback (and optionally submenu).
    Value items:   provide values list + optional on_change callback.
                   up/down cycles the value when the item is selected.
    """

    def __init__(self, label, callback=None, submenu=None,
                 values=None, value_index=0, on_change=None,
                 value_on_new_line=False, live_update=False):
        self.label = label
        self.callback = callback
        self.submenu = submenu          # Reference to another Menu instance
        self.values = values            # list of options; None = action item
        self.value_index = value_index  # index of current value
        self.on_change = on_change      # called with new value on every change
        self.value_on_new_line = value_on_new_line
        self.live_update = live_update  # fire on_change on every press, not just confirm
        self.disabled = False           # when True, item is greyed out and skipped

    def get_value(self):
        """Return the currently selected value, or None for action items."""
        if self.values is not None:
            return self.values[self.value_index]
        return None

    def next_value(self, fire_callback=True):
        """Advance to the next value, optionally firing on_change."""
        if self.values:
            self.value_index = (self.value_index + 1) % len(self.values)
            if fire_callback and self.on_change:
                self.on_change(self.get_value())

    def prev_value(self, fire_callback=True):
        """Step back to the previous value, optionally firing on_change."""
        if self.values:
            self.value_index = (self.value_index - 1) % len(self.values)
            if fire_callback and self.on_change:
                self.on_change(self.get_value())


class NumericMenuItem:
    """
    A menu item that holds a numeric value adjusted by a fixed increment.

    Up increases the value, down decreases it.
    on_change is called with the confirmed value when the user exits editing.
    """

    def __init__(self, label, value=0, increment=1,
                 min_value=None, max_value=None, on_change=None,
                 fmt="{}", value_on_new_line=False, live_update=False):
        """
        Args:
            label:             Display label for the item.
            value:             Starting numeric value.
            increment:         Amount to add/subtract per up/down press.
            min_value:         Optional lower bound (inclusive).
            max_value:         Optional upper bound (inclusive).
            on_change:         Callable called with the confirmed value on exit.
            fmt:               Format string for displaying the value (default "{}").
            value_on_new_line: Show the value on a second row instead of inline.
            live_update:       Fire on_change on every press instead of only on confirm.
        """
        self.label = label
        self.callback = None
        self.submenu = None
        self.values = None      # sentinel used by Menu to detect list-based items
        self.on_change = on_change
        self._value = value
        self.increment = increment
        self.min_value = min_value
        self.max_value = max_value
        self._fmt = fmt
        self.value_on_new_line = value_on_new_line
        self.live_update = live_update  # fire on_change on every press, not just confirm
        self.disabled = False           # when True, item is greyed out and skipped

    def get_value(self):
        """Return the current numeric value."""
        return self._value

    def get_display_value(self):
        """Return the value formatted for display."""
        return self._fmt.format(self._value)

    def increment_value(self, fire_callback=True):
        """Increase the value by one increment, wrapping to min_value if both bounds are set."""
        new_val = self._value + self.increment
        if self.max_value is not None and new_val > self.max_value:
            # Wrap to min if both bounds defined, otherwise clamp
            new_val = self.min_value if self.min_value is not None else self.max_value
        self._value = new_val
        if fire_callback and self.on_change:
            self.on_change(self._value)

    def decrement_value(self, fire_callback=True):
        """Decrease the value by one increment, wrapping to max_value if both bounds are set."""
        new_val = self._value - self.increment
        if self.min_value is not None and new_val < self.min_value:
            # Wrap to max if both bounds defined, otherwise clamp
            new_val = self.max_value if self.max_value is not None else self.min_value
        self._value = new_val
        if fire_callback and self.on_change:
            self.on_change(self._value)


class MaskedStringMenuItem:
    """
    A menu item for editing a structured string made of fixed numeric segments.

    Primarily designed for IP addresses: 4 octets 0-255 separated by '.'.
    In edit mode:
        up / down  — increment / decrement the active segment
        right      — advance to the next segment (or confirm on the last)
        left       — go back to the previous segment (or cancel on the first)
    on_change is called with the complete formatted string when confirmed.
    """

    def __init__(self, label, segments=None, separator=".",
                 seg_min=0, seg_max=255, on_change=None,
                 value_on_new_line=False, live_update=False):
        """
        Args:
            label:             Display label.
            segments:          List of int segment values, e.g. [192, 168, 1, 1].
            separator:         String placed between segments (default ".").
            seg_min:           Minimum value per segment (default 0).
            seg_max:           Maximum value per segment (default 255).
            on_change:         Callable called with the confirmed string on exit.
            value_on_new_line: Show value on a second row instead of inline.
            live_update:       Fire on_change on every segment change, not just confirm.
        """
        self.label = label
        self.callback = None
        self.submenu = None
        self.values = None      # sentinel: not a list-cycling item
        self.on_change = on_change
        self._segments = list(segments) if segments else [0, 0, 0, 0]
        self._original_segments = list(self._segments)
        self.separator = separator
        self.seg_min = seg_min
        self.seg_max = seg_max
        self.value_on_new_line = value_on_new_line
        self.live_update = live_update  # fire on_change on every press, not just confirm
        self.disabled = False           # when True, item is greyed out and skipped
        self._segment_index = 0  # which segment is actively being edited

    def get_value(self):
        """Return the current formatted string, e.g. '192.168.1.1'."""
        return self.separator.join(str(s) for s in self._segments)

    def get_display_value(self):
        """Return the value formatted for display when NOT editing."""
        return self.get_value()

    def get_segment_display(self):
        """Return the value string with the active segment highlighted in [brackets]."""
        parts = []
        for i, s in enumerate(self._segments):
            parts.append("[{}]".format(s) if i == self._segment_index else str(s))
        return self.separator.join(parts)

    def begin_edit(self):
        """Snapshot the current value and reset to the first segment."""
        self._original_segments = list(self._segments)
        self._segment_index = 0

    def cancel_edit(self):
        """Restore the snapshot taken at begin_edit."""
        self._segments = list(self._original_segments)

    def confirm_edit(self):
        """Fire on_change with the confirmed value string."""
        if self.on_change:
            self.on_change(self.get_value())

    def increment_segment(self):
        """Increment the active segment, wrapping to seg_min if both bounds are set."""
        v = self._segments[self._segment_index] + 1
        if self.seg_max is not None and v > self.seg_max:
            v = self.seg_min if self.seg_min is not None else self.seg_max
        self._segments[self._segment_index] = v

    def decrement_segment(self):
        """Decrement the active segment, wrapping to seg_max if both bounds are set."""
        v = self._segments[self._segment_index] - 1
        if self.seg_min is not None and v < self.seg_min:
            v = self.seg_max if self.seg_max is not None else self.seg_min
        self._segments[self._segment_index] = v

    def next_segment(self):
        """Move to next segment. Returns True if moved, False if already on the last."""
        if self._segment_index < len(self._segments) - 1:
            self._segment_index += 1
            return True
        return False

    def prev_segment(self):
        """Move to previous segment. Returns True if moved, False if already on the first."""
        if self._segment_index > 0:
            self._segment_index -= 1
            return True
        return False


class SpacerItem:
    """An empty row that cannot be selected."""
    selectable = False
    value_on_new_line = False


class SeparatorItem:
    """A full-width horizontal rule drawn in the menu text colour."""
    selectable = False
    value_on_new_line = False


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
                 x=5, y=20, width=None, item_height=12,
                 screen_width=160, screen_height=128,
                 bg_color=None, text_color=None,
                 selected_bg=None, selected_text=None,
                 parent_menu=None):
        """
        Initialize the menu.

        Args:
            tft:           ST7735.TFT instance
            dpad:          DPad instance
            title:         Menu title text
            x, y:          Top-left position of the item area
            width:         Width of the menu area in pixels.
                           Defaults to screen_width - 2*x.
            item_height:   Height of each menu item row in pixels (default 12).
            screen_width:  Physical screen width in pixels (default 128).
            screen_height: Physical screen height in pixels (default 160).
                           Used to compute how many item rows fit on screen.
            bg_color:      Background color (default BLACK)
            text_color:    Text color (default WHITE)
            selected_bg:   Selected item background color (default ORANGE)
            selected_text: Selected item text color (default WHITE)
            parent_menu:   Parent menu for back navigation (default None)
        """
        self.tft = tft
        self.dpad = dpad
        self.title = title
        self.x = x
        self.y = y
        self.item_height = item_height

        # Store screen dimensions so they can be inherited by submenus
        self._screen_width = screen_width
        self._screen_height = screen_height

        self.width = width if width is not None else screen_width - (2 * x)

        # Calculate how many item rows fit between y and the bottom of the screen
        self.max_visible_items = max(1, (screen_height - y) // item_height)
        
        # Colors
        self.bg_color = bg_color if bg_color is not None else ST7735.TFT.BLACK
        self.text_color = text_color if text_color is not None else ST7735.TFT.WHITE
        self.selected_bg = selected_bg if selected_bg is not None else ST7735.TFT.ORANGE
        self.selected_text = selected_text if selected_text is not None else ST7735.TFT.WHITE
        # Grey used for disabled items (RGB565: ~128,128,128)
        self.disabled_text_color = 0x8410
        
        # Menu items
        self.items = []
        self.selected_index = 0
        self.visible = False
        
        # Submenu support
        self.parent_menu = parent_menu
        self.active_submenu = None
        
        # Scrolling support
        self.scroll_offset = 0
        
        # Navigation state tracking
        self._last_dpad_state = None
        self._debounce_time = 200      # ms between navigation inputs
        self._hold_threshold = 1000   # ms before auto-repeat activates in edit mode
        self._hold_repeat_delay = 100  # ms between repeats once auto-repeat is active
        self._last_action_time = 0

        # Value-editing mode: index of item whose value is being edited, or None
        self._editing_index = None

    def add_item(self, label, callback=None, submenu=None):
        """Add an action menu item with optional callback or submenu."""
        item = MenuItem(label, callback, submenu)
        self.items.append(item)
        return item

    def add_toggle_item(self, label, on_change=None, initial=False,
                        off_label="Off", on_label="On",
                        value_on_new_line=False, live_update=False):
        """
        Add a boolean toggle item.

        Args:
            label:             Display label for the item.
            on_change:         Callable called with True/False when the value changes.
            initial:           Starting value (True = on, False = off).
            off_label:         Text shown for the False state (default "Off").
            on_label:          Text shown for the True state (default "On").
            value_on_new_line: Show the value on a second row instead of inline.
            live_update:       Fire on_change on every press instead of only on confirm.
        """
        values = [False, True]
        display_values = [off_label, on_label]
        initial_index = 1 if initial else 0

        def _on_change(display_val):
            idx = display_values.index(display_val)
            if on_change:
                on_change(values[idx])

        item = MenuItem(label, values=display_values,
                        value_index=initial_index, on_change=_on_change,
                        value_on_new_line=value_on_new_line, live_update=live_update)
        self.items.append(item)
        return item

    def add_value_item(self, label, values, on_change=None, initial_index=0,
                       value_on_new_line=False, live_update=False):
        """
        Add a value-cycling item with an arbitrary list of options.

        Args:
            label:             Display label for the item.
            values:            List of possible values to cycle through.
            on_change:         Callable called with the new value on each change.
            initial_index:     Index of the starting value.
            value_on_new_line: Show the value on a second row instead of inline.
            live_update:       Fire on_change on every press instead of only on confirm.
        """
        item = MenuItem(label, values=values,
                        value_index=initial_index, on_change=on_change,
                        value_on_new_line=value_on_new_line, live_update=live_update)
        self.items.append(item)
        return item

    def add_numeric_item(self, label, value=0, increment=1,
                         min_value=None, max_value=None,
                         on_change=None, fmt="{}",
                         value_on_new_line=False, live_update=False):
        """
        Add a numeric value item adjusted by up/down presses.

        Args:
            label:             Display label for the item.
            value:             Starting numeric value.
            increment:         Amount to add/subtract per up/down press.
            min_value:         Optional lower bound (inclusive).
            max_value:         Optional upper bound (inclusive).
            on_change:         Callable called with the confirmed value on exit.
            fmt:               Format string for displaying the value, e.g. "{} ms".
            value_on_new_line: Show the value on a second row instead of inline.
            live_update:       Fire on_change on every press instead of only on confirm.
        """
        item = NumericMenuItem(label, value=value, increment=increment,
                               min_value=min_value, max_value=max_value,
                               on_change=on_change, fmt=fmt,
                               value_on_new_line=value_on_new_line, live_update=live_update)
        self.items.append(item)
        return item

    def add_masked_string_item(self, label, segments=None, separator=".",
                               seg_min=0, seg_max=255, on_change=None,
                               value_on_new_line=False, live_update=False):
        """
        Add a segmented string editing item (e.g. IP address).

        Args:
            label:             Display label.
            segments:          List of int segment values (default [0,0,0,0]).
            separator:         Character between segments (default ".").
            seg_min:           Minimum per-segment value (default 0).
            seg_max:           Maximum per-segment value (default 255).
            on_change:         Callable called with the confirmed string on exit.
            value_on_new_line: Show value on a second row instead of inline.
            live_update:       Fire on_change on every segment change, not just confirm.
        """
        item = MaskedStringMenuItem(label, segments=segments, separator=separator,
                                    seg_min=seg_min, seg_max=seg_max,
                                    on_change=on_change,
                                    value_on_new_line=value_on_new_line,
                                    live_update=live_update)
        self.items.append(item)
        return item

    def add_ip_address_item(self, label, ip="0.0.0.0", on_change=None,
                            value_on_new_line=True, live_update=False):
        """
        Convenience helper to add an IPv4 address editing item.

        Args:
            label:             Display label.
            ip:                Starting IP string, e.g. '192.168.1.1'.
            on_change:         Callable called with the confirmed IP string.
            value_on_new_line: Show value on a second row (default True).
            live_update:       Fire on_change on every segment change, not just confirm.
        """
        parts = ip.split(".")
        segments = [int(p) for p in parts] if len(parts) == 4 else [0, 0, 0, 0]
        return self.add_masked_string_item(label, segments=segments, separator=".",
                                           seg_min=0, seg_max=255, on_change=on_change,
                                           value_on_new_line=value_on_new_line,
                                           live_update=live_update)

    def add_submenu(self, label, submenu):
        """Add a submenu item. The submenu's parent will be set to this menu."""
        submenu.parent_menu = self
        self._propagate_screen_to(submenu)
        item = MenuItem(label, submenu=submenu)
        self.items.append(item)
        return item

    def _propagate_screen_to(self, submenu):
        """Copy screen dimensions from this menu to a submenu and recalculate its layout."""
        submenu._screen_width = self._screen_width
        submenu._screen_height = self._screen_height
        # Recalculate derived values based on inherited screen size
        submenu.width = self._screen_width - (2 * submenu.x)
        submenu.max_visible_items = max(1, (self._screen_height - submenu.y) // submenu.item_height)

    def add_spacer(self):
        """Add a blank, non-selectable row."""
        self.items.append(SpacerItem())

    def add_separator(self):
        """Add a full-width horizontal line (non-selectable)."""
        self.items.append(SeparatorItem())

    def set_item_disabled(self, item, disabled):
        """
        Enable or disable a menu item.

        Args:
            item:     The item object returned by an add_* method.
            disabled: True to grey out and skip the item, False to re-enable it.
        """
        item.disabled = disabled
        # If the item is currently selected but is being disabled, move selection
        if disabled and self.visible:
            idx = self.items.index(item)
            if idx == self.selected_index:
                # Find next selectable item
                new_index = self.selected_index + 1
                while new_index < len(self.items) and not self._is_selectable(self.items[new_index]):
                    new_index += 1
                if new_index >= len(self.items):
                    new_index = self.selected_index - 1
                    while new_index >= 0 and not self._is_selectable(self.items[new_index]):
                        new_index -= 1
                if new_index >= 0:
                    old = self.selected_index
                    self.selected_index = new_index
                    self._draw_item(old, False)
                    self._draw_item(new_index, True)
                    return
            if self.visible:
                self._draw_item(idx, idx == self.selected_index)
        elif self.visible:
            idx = self.items.index(item)
            self._draw_item(idx, idx == self.selected_index)
        
    def clear_items(self):
        """Remove all menu items."""
        self.items.clear()
        self.selected_index = 0
        self.scroll_offset = 0
        self.active_submenu = None
        
    def show(self):
        """Display the menu on the screen."""
        self.visible = True
        # Always start from the top when (re-)opening the menu
        self.scroll_offset = 0
        self.selected_index = 0
        # Advance past any leading non-selectable items
        while (self.selected_index < len(self.items) and
               not self._is_selectable(self.items[self.selected_index])):
            self.selected_index += 1
        self._draw_full_menu()
        
    def hide(self):
        """Hide the menu (clear the screen)."""
        self.visible = False
        self.tft.fill(self.bg_color)
        
    def _has_items_below(self):
        """Return True if any items extend below the visible area."""
        area_bottom = self.y + self._visible_area_height()
        test_y = self.y
        for i in range(self.scroll_offset, len(self.items)):
            h = self._item_height(self.items[i])
            test_y += h
            if test_y > area_bottom:
                return True
        return False

    def _draw_full_menu(self):
        """Draw the entire menu including title, scroll indicators and all visible items.

        Avoids a full-screen tft.fill() to prevent the black-flash flicker.
        Instead, only the title bar strip is cleared; each item overwrites its
        own row via fillrect inside _draw_item.
        """
        # Clear only the title bar strip (above the item area)
        self.tft.fillrect((0, 0), (self._screen_width, self.y), self.bg_color)

        # Title
        title_y = self.y - 15
        self.tft.text((self.x, title_y), self.title,
                      self.text_color, font5x8.font5x8, bgColor=self.bg_color)

        # Separator line
        separator = "-" * (self.width // 6)
        self.tft.text((self.x, title_y + 10), separator,
                      self.text_color, font5x8.font5x8, bgColor=self.bg_color)

        # Draw menu items; track the bottom of the last drawn item
        area_top = self.y
        area_bottom = self.y + self._visible_area_height()
        last_item_bottom = area_top

        for i in range(len(self.items)):
            self._draw_item(i, i == self.selected_index)
            if i >= self.scroll_offset:
                iy = self._item_y(i)
                ih = self._item_height(self.items[i])
                bottom = iy + ih
                if bottom <= area_bottom and bottom > last_item_bottom:
                    last_item_bottom = bottom

        # Clear any empty rows below the last item within the visible area
        if last_item_bottom < area_bottom:
            self.tft.fillrect(
                (0, last_item_bottom),
                (self._screen_width, area_bottom - last_item_bottom),
                self.bg_color)

        # Draw scroll indicators last so they sit on top of any edge overdraw
        self._draw_scroll_indicators()
    
    def _is_selectable(self, item):
        """Return True if the item can receive navigation focus."""
        if not getattr(item, 'selectable', True):
            return False
        if getattr(item, 'disabled', False):
            return False
        return True

    def _item_height(self, item):
        """Return the pixel height of an item (1 or 2 rows)."""
        if getattr(item, 'value_on_new_line', False):
            return self.item_height * 2
        return self.item_height

    def _item_y(self, index):
        """Return the absolute Y pixel position of item[index]."""
        y = self.y
        for i in range(self.scroll_offset, index):
            y += self._item_height(self.items[i])
        return y

    def _visible_area_height(self):
        """Total pixel height available for items."""
        return self.item_height * self.max_visible_items

    def _draw_item(self, index, is_selected):
        """Draw a single menu item."""
        if index < self.scroll_offset:
            return

        item = self.items[index]
        item_y = self._item_y(index)
        h = self._item_height(item)

        # Check item is within the visible area
        if item_y + h > self.y + self._visible_area_height():
            return

        editing = (self._editing_index == index)
        disabled = getattr(item, 'disabled', False)

        # Choose colours based on selection / editing / disabled state
        if disabled:
            bg = self.bg_color
            fg = self.disabled_text_color
            prefix = " "
        elif editing:
            bg = ST7735.TFT.CYAN
            fg = ST7735.TFT.BLACK
            prefix = "*"
        elif is_selected:
            bg = self.selected_bg
            fg = self.selected_text
            prefix = ">"
        else:
            bg = self.bg_color
            fg = self.text_color
            prefix = " "

        # Clear the full item area (1 or 2 rows)
        self.tft.fillrect((self.x, item_y), (self.width, h), bg)

        # Spacer — just an empty row
        if isinstance(item, SpacerItem):
            return

        # Separator — a solid horizontal rule
        if isinstance(item, SeparatorItem):
            mid_y = item_y + h // 2
            self.tft.fillrect((self.x, mid_y), (self.width, 1), self.text_color)
            return

        # Get the value display string (for value items)
        if isinstance(item, MaskedStringMenuItem):
            # Active segment is shown with [brackets] inside get_segment_display()
            val_display = item.get_segment_display() if editing else "[{}]".format(item.get_value())
        elif isinstance(item, NumericMenuItem):
            raw = item.get_display_value()
            val_display = "<{}>".format(raw) if editing else "[{}]".format(raw)
        elif item.values is not None:
            raw = str(item.get_value())
            val_display = "<{}>".format(raw) if editing else "[{}]".format(raw)
        else:
            val_display = None

        if val_display is not None and item.value_on_new_line:
            # Row 1: prefix + label
            self.tft.text((self.x + 2, item_y + 2),
                          f"{prefix} {item.label}", fg, font5x8.font5x8, bgColor=bg)
            # Row 2: indented value
            self.tft.text((self.x + 10, item_y + self.item_height + 2),
                          val_display, fg, font5x8.font5x8, bgColor=bg)
        elif val_display is not None:
            # Single row: label + value inline
            self.tft.text((self.x + 2, item_y + 2),
                          f"{prefix} {item.label}: {val_display}",
                          fg, font5x8.font5x8, bgColor=bg)
        else:
            # Action item — label only
            self.tft.text((self.x + 2, item_y + 2),
                          f"{prefix} {item.label}", fg, font5x8.font5x8, bgColor=bg)

    def _bg_color_at(self, screen_y):
        """Return the background colour currently drawn at the given screen y coordinate.

        Used so scroll indicators can blend with whatever item row is beneath them.
        """
        for i in range(self.scroll_offset, len(self.items)):
            iy = self._item_y(i)
            ih = self._item_height(self.items[i])
            if iy <= screen_y < iy + ih:
                if i == self._editing_index:
                    return ST7735.TFT.CYAN
                elif i == self.selected_index:
                    return self.selected_bg
                else:
                    return self.bg_color
            if iy > screen_y:
                break
        return self.bg_color

    def _draw_scroll_indicators(self):
        """Draw ^ / v scroll arrows in the top-right and bottom-right screen corners.

        Fills the full item-row height at the indicator column before drawing the
        character, so no gaps appear at the top/bottom edges of a highlighted row.
        """
        char_w = 6   # approximate character width in pixels
        char_h = 8   # approximate character height in pixels
        ind_x = self._screen_width - char_w - 1

        # Top-right corner — always above the item area, background is always bg_color
        top_y = 1
        top_char = "^" if self.scroll_offset > 0 else " "
        self.tft.text((ind_x, top_y), top_char,
                      self.text_color, font5x8.font5x8, bgColor=self.bg_color)

        # Bottom-right corner — may overlap a highlighted item row.
        # Find which item (if any) the indicator column sits in, fill the full row
        # height at that column, then draw the character centred within the row.
        bot_char = "v" if self._has_items_below() else " "
        bot_y_fallback = self._screen_height - char_h - 1
        drawn = False
        for i in range(self.scroll_offset, len(self.items)):
            iy = self._item_y(i)
            ih = self._item_height(self.items[i])
            # Stop searching once we're past the visible area
            if iy >= self.y + self._visible_area_height():
                break
            # Check whether the indicator's natural position falls inside this row
            if iy <= bot_y_fallback < iy + ih:
                if i == self._editing_index:
                    row_bg = ST7735.TFT.CYAN
                elif i == self.selected_index:
                    row_bg = self.selected_bg
                else:
                    row_bg = self.bg_color
                # Fill the full item-row height at the indicator column
                self.tft.fillrect((ind_x, iy), (char_w, ih), row_bg)
                # Centre the character vertically within the row
                char_y = iy + (ih - char_h) // 2
                self.tft.text((ind_x, char_y), bot_char,
                              self.text_color, font5x8.font5x8, bgColor=row_bg)
                drawn = True
                break
        if not drawn:
            # Fallback: indicator is beyond item area — plain background
            self.tft.fillrect((ind_x, bot_y_fallback), (char_w, char_h), self.bg_color)
            self.tft.text((ind_x, bot_y_fallback), bot_char,
                          self.text_color, font5x8.font5x8, bgColor=self.bg_color)

    def _update_selection(self, old_index, new_index):
        """Update the display when selection changes (partial redraw, no flicker)."""
        self._draw_item(old_index, False)
        self._draw_item(new_index, True)
        # Redraw corners in case an item's fillrect reached a screen edge
        self._draw_scroll_indicators()

    def _handle_scroll(self):
        """Adjust scroll offset to keep selected item visible, accounting for variable item heights."""
        area_h = self._visible_area_height()
        old_offset = self.scroll_offset

        # Scroll down: selected item's bottom is past the visible area
        while True:
            item_y = self._item_y(self.selected_index)
            item_h = self._item_height(self.items[self.selected_index])
            if item_y + item_h > self.y + area_h:
                self.scroll_offset += 1
            else:
                break

        # Scroll up: selected item is above the visible area
        if self.selected_index < self.scroll_offset:
            self.scroll_offset = self.selected_index

        # Only redraw if the scroll position actually changed
        if self.scroll_offset != old_offset:
            self._draw_full_menu()
    
    def update(self):
        """
        Update menu state based on D-pad input.
        Call this in your main loop.

        Returns:
            True if an action item was activated, False otherwise.
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

        # ------------------------------------------------------------------ #
        # VALUE-EDITING MODE                                                   #
        # Up/down cycles the value; right or left confirms and exits.          #
        # ------------------------------------------------------------------ #
        if self._editing_index is not None:
            item = self.items[self._editing_index]

            # ---------------------------------------------------------------- #
            # MaskedStringMenuItem: segment-by-segment navigation              #
            #   up/down  — increment/decrement active segment                  #
            #   right    — next segment, or confirm if on the last             #
            #   left     — prev segment, or cancel if on the first             #
            # ---------------------------------------------------------------- #
            if isinstance(item, MaskedStringMenuItem):
                up_state   = dpad_state["up"]["state"]
                up_held    = dpad_state["up"]["held_ms"]
                down_state = dpad_state["down"]["state"]
                down_held  = dpad_state["down"]["held_ms"]

                def _edit_delay(held):
                    return self._hold_repeat_delay if held > self._hold_threshold else self._debounce_time

                if (up_state == "button_down" or
                        (up_state == "pressed" and up_held > self._hold_threshold)):
                    if time.ticks_diff(current_time, self._last_action_time) > _edit_delay(up_held):
                        item.increment_segment()
                        if item.live_update and item.on_change:
                            item.on_change(item.get_value())
                        self._draw_item(self._editing_index, True)
                        self._last_action_time = current_time

                elif (down_state == "button_down" or
                        (down_state == "pressed" and down_held > self._hold_threshold)):
                    if time.ticks_diff(current_time, self._last_action_time) > _edit_delay(down_held):
                        item.decrement_segment()
                        if item.live_update and item.on_change:
                            item.on_change(item.get_value())
                        self._draw_item(self._editing_index, True)
                        self._last_action_time = current_time

                elif dpad_state["right"]["state"] == "button_down":
                    if time.ticks_diff(current_time, self._last_action_time) > self._debounce_time:
                        if not item.next_segment():
                            # Last segment — confirm and exit
                            # Only fire on_change here if not live_update (live already fired)
                            if not item.live_update:
                                item.confirm_edit()
                            self._editing_index = None
                            self._draw_item(self.selected_index, True)
                        else:
                            self._draw_item(self._editing_index, True)
                        self._last_action_time = current_time

                elif dpad_state["left"]["state"] == "button_down":
                    if time.ticks_diff(current_time, self._last_action_time) > self._debounce_time:
                        if not item.prev_segment():
                            # First segment — cancel and restore original
                            item.cancel_edit()
                            # Restore live listeners to original value if needed
                            if item.live_update and item.on_change:
                                item.on_change(item.get_value())
                            self._editing_index = None
                            self._draw_item(self.selected_index, True)
                        else:
                            self._draw_item(self._editing_index, True)
                        self._last_action_time = current_time

                return False

            # ---------------------------------------------------------------- #
            # Standard value / numeric editing                                 #
            #   up/down  — cycle value (with auto-repeat after 1 s held)       #
            #   right/left — confirm and exit                                  #
            # ---------------------------------------------------------------- #
            up_state   = dpad_state["up"]["state"]
            up_held    = dpad_state["up"]["held_ms"]
            down_state = dpad_state["down"]["state"]
            down_held  = dpad_state["down"]["held_ms"]

            def _edit_delay(held):
                return self._hold_repeat_delay if held > self._hold_threshold else self._debounce_time

            if (up_state == "button_down" or
                    (up_state == "pressed" and up_held > self._hold_threshold)):
                if time.ticks_diff(current_time, self._last_action_time) > _edit_delay(up_held):
                    if isinstance(item, NumericMenuItem):
                        item.increment_value(fire_callback=item.live_update)
                    else:
                        item.prev_value(fire_callback=item.live_update)
                    self._draw_item(self._editing_index, True)
                    self._last_action_time = current_time

            elif (down_state == "button_down" or
                    (down_state == "pressed" and down_held > self._hold_threshold)):
                if time.ticks_diff(current_time, self._last_action_time) > _edit_delay(down_held):
                    if isinstance(item, NumericMenuItem):
                        item.decrement_value(fire_callback=item.live_update)
                    else:
                        item.next_value(fire_callback=item.live_update)
                    self._draw_item(self._editing_index, True)
                    self._last_action_time = current_time

            elif (dpad_state["right"]["state"] == "button_down" or
                  dpad_state["left"]["state"] == "button_down"):
                if time.ticks_diff(current_time, self._last_action_time) > self._debounce_time:
                    # Confirm: fire on_change only if not live_update (live already fired)
                    if not item.live_update and item.on_change:
                        item.on_change(item.get_value())
                    self._editing_index = None
                    self._draw_item(self.selected_index, True)
                    self._last_action_time = current_time

            return False

        # ------------------------------------------------------------------ #
        # NORMAL NAVIGATION MODE                                               #
        # ------------------------------------------------------------------ #

        # Check for up navigation
        if dpad_state["up"]["state"] == "button_down":
            if time.ticks_diff(current_time, self._last_action_time) > self._debounce_time:
                old_index = self.selected_index
                new_index = self.selected_index - 1
                while new_index >= 0 and not self._is_selectable(self.items[new_index]):
                    new_index -= 1
                if new_index < 0:
                    # Wrap: find the last selectable item
                    new_index = len(self.items) - 1
                    while new_index > old_index and not self._is_selectable(self.items[new_index]):
                        new_index -= 1
                self.selected_index = new_index
                if old_index != self.selected_index:
                    self._update_selection(old_index, self.selected_index)
                    self._handle_scroll()
                self._last_action_time = current_time

        # Check for down navigation
        elif dpad_state["down"]["state"] == "button_down":
            if time.ticks_diff(current_time, self._last_action_time) > self._debounce_time:
                old_index = self.selected_index
                new_index = self.selected_index + 1
                while new_index < len(self.items) and not self._is_selectable(self.items[new_index]):
                    new_index += 1
                if new_index >= len(self.items):
                    # Wrap: find the first selectable item
                    new_index = 0
                    while new_index < old_index and not self._is_selectable(self.items[new_index]):
                        new_index += 1
                self.selected_index = new_index
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
        """Activate the currently selected item.

        - Value items  → enter value-editing mode.
        - Submenu items → open the submenu.
        - Action items  → execute the callback.
        """
        if 0 <= self.selected_index < len(self.items):
            item = self.items[self.selected_index]

            if isinstance(item, MaskedStringMenuItem):
                # Snapshot current value and enter segment-by-segment editing
                item.begin_edit()
                self._editing_index = self.selected_index
                self._draw_item(self.selected_index, True)
            elif item.values is not None or isinstance(item, NumericMenuItem):
                # Enter editing mode for this item
                self._editing_index = self.selected_index
                self._draw_item(self.selected_index, True)
            elif item.submenu is not None:
                self.open_submenu(item.submenu)
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
        """Open a submenu, inheriting screen dimensions from this menu."""
        self.active_submenu = submenu
        submenu.parent_menu = self
        self._propagate_screen_to(submenu)
        submenu.show()
    
    def go_back(self):
        """Go back to parent menu."""
        if self.parent_menu is not None:
            self.hide()
            self.parent_menu.active_submenu = None
            self.parent_menu.show()
    
    def add_back_item(self, label="< Back"):
        """Convenience method to add a back item that returns to parent menu."""
        self.add_item(label, lambda: self.go_back())
