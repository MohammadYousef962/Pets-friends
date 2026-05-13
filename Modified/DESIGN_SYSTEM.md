# PetsFriends - Design System & Component Reference

## 🎨 Color Palette

### Primary Colors
- **Primary Brown**: `#8B6F47` - Main brand color
- **Light Brown**: `#A0826D` - Hover states, lighter elements
- **Cream**: `#F5F1EB` - Background sections
- **Accent Gold**: `#C8A882` - Decorative elements (paw prints)

### Neutral Colors
- **Dark Text**: `#2C2C2C` - Headlines
- **Muted Text**: `#666666` - Body text, secondary text
- **Light Border**: `#E8E8E8` - Dividers, subtle borders
- **White**: `#FFFFFF` - Backgrounds, cards

### Status Colors
- **Danger**: `#DC3545` - Logout button, alerts
- **Success**: `#28A745` - Positive actions (if needed)

---

## 🔤 Typography

### Font Stack (Bootstrap Default)
```css
font-family: system-ui, -apple-system, "Segoe UI", Roboto, "Helvetica Neue", Arial, sans-serif;
```

### Size Scale
- **Hero Title**: `clamp(2.5rem, 6vw, 3.5rem)` - Responsive scaling
- **Heading 2**: `clamp(1.6rem, 3vw, 2.2rem)` - Section headings
- **Body Large**: `0.95rem` - Main text
- **Body Regular**: `0.9rem` - Secondary text
- **Small**: `0.85rem` - Compact buttons, labels

### Weight
- **Regular**: 400
- **Medium**: 500
- **Semibold**: 600
- **Bold**: 700
- **Black**: 900 - Hero title

### Line Height
- **Tight**: 1.1 - Headlines
- **Relaxed**: 1.75 - Body text
- **Normal**: 1.5 - Default

---

## 🟦 Component Dimensions

### Top Bar
- Height (Desktop): `70px`
- Height (Tablet): `65px`
- Height (Mobile): `60px`
- Brand Icon Size: `1.8rem`
- Brand Text Size: `1.25rem`

### Hero Section
- Min Height (Desktop): `90vh`
- Pet Images Max Width: `280px`
- Pet Containers Height: `400px`
- Padding (Desktop): `80px 20px 60px`
- Padding (Mobile): `40px 16px 30px`

### Buttons
- Button Height: `44px` (standard)
- Padding Horizontal: `38px`
- Padding Vertical: `14px`
- Border Radius: `28px`
- Compact: `12px 28px`

### Search Box
- Height: `40px`
- Width: `140px`
- Border Radius: `24px`
- Padding: `8px 12px`

---

## 🎬 Animation Specifications

### Hero Pet Float
```
Duration: 3s
Timing: ease-in-out
Distance: ±20px vertical
Delay: Staggered (0s, 0.5s)
```

### Paw Print Float
```
Duration: 4s
Timing: ease-in-out
Distance: ±15px vertical, ±5deg rotation
Delay: Staggered (0s, 0.5s, 1s, 1.5s)
```

### Button Ripple
```
Duration: 0.6s
Type: Radial expansion
Speed: Linear
Color: rgba(255, 255, 255, 0.2)
```

### Hover Effects
- Transform: `translateY(-3px)` - Lift effect
- Transition: `all 0.3s cubic-bezier(0.34, 1.56, 0.64, 1)` - Smooth spring
- Shadow: Increased `0 12px 32px rgba(139, 111, 71, 0.35)`

### Navigation Link Underline
```
Duration: 0.3s
From: width: 0
To: width: 20px
Position: Bottom 16px
```

---

## 📐 Spacing & Grid

### Container Max-Width
- Desktop: `1400px`

### Padding Standards
- **Section**: `64px 80px`
- **Container**: `20px` horizontal
- **Card**: `24px 22px`
- **Button**: `14px 38px` (large), `8px 16px` (small)

### Gap Standards
- **Navigation Items**: `18px`
- **Hero Grid Gap**: `40px`
- **Service Grid**: `18px`
- **Flex Gaps**: `8px` - `20px` depending on context

---

## 🎯 Responsive Grid

### Desktop (1025px+)
```
Hero Container: grid-template-columns: 1fr 1.2fr 1fr
Services Grid: grid-template-columns: repeat(3, 1fr)
Navigation: Full visible
Search: Visible
```

### Tablet (768px - 1024px)
```
Hero Container: grid-template-columns: 1fr
Services Grid: grid-template-columns: repeat(2, 1fr)
Navigation: Collapsed menu
Search: Hidden
Pet Images: Hidden
```

### Mobile (Below 768px)
```
Hero Container: grid-template-columns: 1fr
Services Grid: grid-template-columns: 1fr
Navigation: Hamburger menu
Search: Hidden on nav
Pet Images: Hidden
Padding: 40px 16px
```

---

## 🎪 Shadow System

### Light Shadow (Default)
```css
box-shadow: 0 2px 8px rgba(0, 0, 0, 0.08);
```

### Medium Shadow (Hover States)
```css
box-shadow: 0 8px 28px rgba(92, 61, 30, 0.11);
```

### Strong Shadow (Primary Button)
```css
box-shadow: 0 8px 24px rgba(139, 111, 71, 0.25);
```

### Heavy Shadow (Hover Button)
```css
box-shadow: 0 12px 32px rgba(139, 111, 71, 0.35);
```

### Subtle Shadow (Pet Images)
```css
filter: drop-shadow(0 12px 32px rgba(92, 61, 30, 0.15));
```

---

## 🔘 Button Styles

### Primary Button (Hero)
```
Background: Linear gradient (Brown → Light Brown)
Text Color: White
Border Radius: 28px
Padding: 14px 38px
Font Weight: 600
Font Size: 0.95rem
Hover: Transform translateY(-3px), shadow increase
Active: Background darken
```

### Authentication Buttons
```
Background: Primary brown or danger red
Padding: 8px 16px (compact)
Border Radius: 20px (more rounded)
Font Size: 0.9rem
Flex with icon and text
```

### Navigation Links
```
Color: Muted gray
Font Weight: 500
Font Size: 0.95rem
Hover: Color change to brown
Underline: Animated from center
```

---

## 📏 Border & Outline

### Standard Border
```css
border: 1px solid rgba(200, 168, 130, 0.18);
```

### Focus States
```css
outline: 2px solid var(--primary-brown);
outline-offset: 2px;
```

### Card Border Radius
```css
border-radius: 16px;
```

### Button Border Radius
```css
border-radius: 28px;
```

### Icon Wrap Border Radius
```css
border-radius: 12px;
```

---

## 🔌 CSS Variables Quick Reference

```css
:root {
    --topbar-bg: #ffffff;
    --topbar-text: #1a1a1a;
    --topbar-border: #e8e8e8;
    --primary-color: #8B6F47;
    --primary-light: #A0826D;
    --text-muted: #666666;
    --shadow-light: 0 2px 8px rgba(0, 0, 0, 0.08);

    --primary-brown: #8B6F47;
    --primary-light: #A0826D;
    --cream-bg: #F5F1EB;
    --text-dark: #2C2C2C;
    --text-muted: #666666;
    --accent-gold: #C8A882;
}
```

---

## 🎨 Gradient Definitions

### Title Gradient (Hero Title)
```css
background: linear-gradient(135deg, #8B6F47 0%, #C8A882 100%);
-webkit-background-clip: text;
-webkit-text-fill-color: transparent;
background-clip: text;
```

### Button Gradient
```css
background: linear-gradient(135deg, #8B6F47 0%, #A0826D 100%);
```

---

## ♿ Accessibility Features

- **Focus States**: All interactive elements have visible focus indicators
- **ARIA Labels**: Buttons have proper aria-labels
- **Semantic HTML**: Proper heading hierarchy (h1, h2, h3)
- **Color Contrast**: Text meets WCAG AA standards
- **Touch Targets**: Minimum 44px height for buttons
- **Skip Links**: Navigate directly to main content (if implemented)
- **Alt Text**: All images have descriptive alt text

---

## 🚀 Performance Notes

- **CSS**: Organized by component, easy to tree-shake
- **Animations**: Use CSS transforms (GPU accelerated)
- **Images**: Lazy loading ready (add `loading="lazy"`)
- **JavaScript**: Minimal dependencies, vanilla JS
- **Bundle Size**: ~15KB CSS + ~3KB JS (unminified)

---

## 🔗 File Dependencies

```
Views/Shared/_Layout.cshtml
├── css/topbar.css (Modern top bar)
├── css/site.css (Bootstrap & base styles)
├── css/landing.css (Hero & sections)
├── js/topbar.js (Interactions)
└── Bootstrap Icons (CDN)

Views/Home/Index.cshtml
├── css/landing.css
└── Hero section markup
```

---

## 📞 Quick Customization Checklist

- [ ] Update logo/brand text
- [ ] Replace pet images
- [ ] Update navigation links
- [ ] Customize colors in CSS variables
- [ ] Update button text/links
- [ ] Implement search functionality
- [ ] Add your phone number/email
- [ ] Update footer information
- [ ] Test on mobile devices
- [ ] Test on different browsers

---

Created with ❤️ for PetsFriends | Modern, responsive, and performant UI/UX
