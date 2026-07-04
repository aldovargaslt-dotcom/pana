# Tailwind CSS Cheatsheet

Quick reference for Tailwind CSS v4 utility classes. For full docs, see [tailwindcss.com/docs](https://tailwindcss.com/docs/).

## Layout

### Display
```
block inline inline-block flex inline-flex grid inline-grid hidden
```

### Flexbox
```
flex-row flex-col flex-row-reverse flex-col-reverse
flex-wrap flex-nowrap flex-wrap-reverse
flex-1 flex-auto flex-initial flex-none
grow grow-0 shrink shrink-0
```

### Flex Alignment
```
justify-start justify-end justify-center justify-between justify-around justify-evenly
items-start items-end items-center items-baseline items-stretch
content-start content-end content-center content-between content-around
self-auto self-start self-end self-center self-stretch
```

### Grid
```
grid grid-cols-1 ... grid-cols-12 grid-cols-none
grid-rows-1 ... grid-rows-6 grid-rows-none
grid-flow-row grid-flow-col grid-flow-dense
auto-cols-auto auto-cols-min auto-cols-max auto-cols-fr
auto-rows-auto auto-rows-min auto-rows-max auto-rows-fr
```

### Grid Span / Placement
```
col-span-1 ... col-span-12 col-span-full
col-start-1 ... col-start-13 col-start-auto
col-end-1 ... col-end-13 col-end-auto
row-span-1 ... row-span-6 row-span-full
```

### Gap
```
gap-0 gap-1 gap-2 gap-3 gap-4 gap-5 gap-6 gap-8 gap-10 gap-12 gap-16
gap-x-* gap-y-*
```

### Position
```
static fixed absolute relative sticky
inset-0 inset-x-0 inset-y-0
top-0 right-0 bottom-0 left-0
z-0 z-10 z-20 z-30 z-40 z-50 z-auto
```

## Spacing

### Padding & Margin
```
p-0 p-1 p-2 p-3 p-4 p-5 p-6 p-8 p-10 p-12 p-16 p-20 p-24
px-* py-* pt-* pr-* pb-* pl-*
m-0 ... m-auto
mx-* my-* mt-* mr-* mb-* ml-*
```

### Space Between (flex/grid children)
```
space-x-0 ... space-x-8 space-x-reverse
space-y-0 ... space-y-8 space-y-reverse
```

## Sizing

### Width & Height
```
w-0 w-1 ... w-96 w-auto w-full w-screen w-min w-max w-fit
w-1/2 w-1/3 w-2/3 w-1/4 w-3/4 w-1/5 w-2/5 w-3/5 w-4/5
h-0 ... h-96 h-auto h-full h-screen h-min h-max h-fit
size-*  (sets both width and height)
min-w-0 min-w-full min-w-min min-w-max min-w-fit
max-w-xs max-w-sm max-w-md max-w-lg max-w-xl max-w-2xl ... max-w-7xl max-w-full
min-h-0 min-h-full min-h-screen
max-h-* max-h-full max-h-screen
```

## Typography

### Font Size
```
text-xs text-sm text-base text-lg text-xl text-2xl text-3xl text-4xl
text-5xl text-6xl text-7xl text-8xl text-9xl
```

### Font Weight
```
font-thin font-extralight font-light font-normal font-medium
font-semibold font-bold font-extrabold font-black
```

### Font Style & Family
```
italic not-italic
font-sans font-serif font-mono
```

### Letter Spacing
```
tracking-tighter tracking-tight tracking-normal tracking-wide tracking-wider tracking-widest
```

### Line Height
```
leading-none leading-tight leading-snug leading-normal leading-relaxed leading-loose
leading-3 ... leading-10
```

### Text Alignment
```
text-left text-center text-right text-justify text-start text-end
```

### Text Decoration
```
underline line-through no-underline
decoration-* decoration-solid decoration-double decoration-dotted decoration-dashed
decoration-wavy
underline-offset-*
```

### Text Transform
```
uppercase lowercase capitalize normal-case
```

### Text Overflow
```
truncate text-ellipsis text-clip
```

### Whitespace
```
whitespace-normal whitespace-nowrap whitespace-pre whitespace-pre-line whitespace-pre-wrap
```

### Vertical Align
```
align-baseline align-top align-middle align-bottom align-text-top align-text-bottom align-sub align-super
```

## Colors

### Background
```
bg-transparent bg-current bg-black bg-white
bg-gray-50 ... bg-gray-950
bg-red-50 ... bg-red-950
bg-brand-50 ... bg-brand-950   (Pana custom palette)
```

### Text
```
text-transparent text-current text-black text-white
text-gray-50 ... text-gray-950
text-brand-50 ... text-brand-950
```

### Border
```
border-transparent border-current border-black border-white
border-gray-50 ... border-gray-950
border-brand-50 ... border-brand-950
```

See [full Tailwind color palette](https://tailwindcss.com/docs/colors).

## Borders

### Border Width
```
border border-0 border-2 border-4 border-8
border-t border-r border-b border-l
border-x border-y
```

### Border Radius
```
rounded-none rounded-sm rounded rounded-md rounded-lg rounded-xl rounded-2xl rounded-3xl rounded-full
rounded-t-* rounded-r-* rounded-b-* rounded-l-*
rounded-tl-* rounded-tr-* rounded-br-* rounded-bl-*
```

### Border Style
```
border-solid border-dashed border-dotted border-double border-hidden border-none
```

### Divide (between flex/grid children)
```
divide-x divide-x-2 divide-x-4 divide-x-8
divide-y divide-y-2 divide-y-4 divide-y-8
divide-gray-200 divide-transparent ...
```

### Outline
```
outline-none outline outline-2 outline-4 outline-8
outline-offset-0 outline-offset-2 outline-offset-4
outline-brand-500 ...
```

### Ring
```
ring-0 ring-1 ring-2 ring-4 ring-8 ring-inset
ring-brand-500 ring-gray-200 ...
ring-offset-0 ring-offset-2 ring-offset-4
```

## Effects

### Box Shadow
```
shadow-sm shadow shadow-md shadow-lg shadow-xl shadow-2xl shadow-none
shadow-soft shadow-glow shadow-card     (Pana custom)
shadow-brand-500/50 ...                 (colored shadows with opacity)
```

### Opacity
```
opacity-0 opacity-5 opacity-10 ... opacity-95 opacity-100
```

### Background Blend Mode
```
bg-blend-normal bg-blend-multiply bg-blend-screen bg-blend-overlay
bg-blend-darken bg-blend-lighten bg-blend-color-dodge bg-blend-color-burn
```

### Mix Blend Mode
```
mix-blend-normal mix-blend-multiply mix-blend-screen mix-blend-overlay
```

### Backdrop Filters
```
backdrop-blur-none backdrop-blur-sm backdrop-blur backdrop-blur-md
backdrop-blur-lg backdrop-blur-xl backdrop-blur-2xl backdrop-blur-3xl
backdrop-brightness-* backdrop-contrast-*
```

## Filters

```
blur-none blur-sm blur blur-md blur-lg blur-xl blur-2xl blur-3xl
brightness-* contrast-* grayscale grayscale-0
hue-rotate-* invert invert-0
saturate-* sepia sepia-0
```

## Transitions & Animation

### Transition
```
transition-none transition-all transition transition-colors transition-opacity
transition-shadow transition-transform
duration-75 duration-100 duration-150 duration-200 duration-300 duration-500 duration-700 duration-1000
ease-linear ease-in ease-out ease-in-out
delay-75 delay-100 delay-150 delay-200 delay-300 delay-500 delay-700 delay-1000
```

### Animation
```
animate-none animate-spin animate-ping animate-pulse animate-bounce
```
Pana custom animations: `fade-in`, `slide-in-right`, `scale-in`, `skeleton`, `text-gradient`

## Interactivity

### Cursor
```
cursor-auto cursor-default cursor-pointer cursor-wait cursor-text cursor-move cursor-not-allowed
```

### User Select
```
select-none select-text select-all select-auto
```

### Pointer Events
```
pointer-events-none pointer-events-auto
```

### Scroll Behavior
```
scroll-smooth scroll-auto
```

### Resize
```
resize-none resize resize-y resize-x
```

## State Variants

### Pseudo-classes
```
hover: focus: focus-within: focus-visible: active: visited: target:
first: last: only: odd: even:
first-of-type: last-of-type: only-of-type:
empty: disabled: enabled: checked: indeterminate: default:
required: optional: valid: invalid: user-valid: user-invalid:
in-range: out-of-range: placeholder-shown: autofill: read-only:
```

### Parent state (group)
```html
<div class="group">
    <span class="group-hover:text-brand-600">Hover me</span>
</div>
```
```
group-hover: group-focus: group-active: group-odd: group-even:
group-aria-*: group-has-*:
```

### Named groups
```html
<div class="group/item">
    <a class="group-hover/item:visible ...">...</a>
</div>
```

### Sibling state (peer)
```html
<input class="peer" type="checkbox" />
<p class="peer-checked:block hidden">Checked!</p>
```
```
peer-hover: peer-focus: peer-checked: peer-invalid: peer-required:
peer-disabled: peer-has-*:
```

### Descendant state (has)
```
has-checked: has-[:focus]: has-[img]: has-[a]:
```

### Attribute selectors
```
aria-checked: aria-expanded: aria-selected: aria-[sort=ascending]:
data-[size=large]: data-active:
open: inert:
```

### Child selectors
```html
<ul class="*:rounded-full *:border *:px-2 *:py-0.5">
    <li>Item</li>
</ul>
```
```
*: **:
```

## Responsive (mobile-first)

| Prefix | Min width |
|--------|-----------|
| (none) | All sizes (mobile default) |
| `sm:` | 640px |
| `md:` | 768px |
| `lg:` | 1024px |
| `xl:` | 1280px |
| `2xl:` | 1536px |

### Max-width variants
```
max-sm: max-md: max-lg: max-xl: max-2xl:
```

### Arbitrary breakpoints
```
min-[320px]: max-[600px]:
```

### Container queries
```html
<div class="@container">
    <div class="flex flex-col @md:flex-row">...</div>
</div>
```
```
@container @3xs ... @7xl @min-[475px] @max-[960px]
@sm/main  (named containers)
```

## Dark Mode
```
dark:  — targets prefers-color-scheme: dark
```

```html
<div class="bg-white dark:bg-gray-900 text-gray-900 dark:text-gray-100">
```

## Accessibility Variants
```
motion-safe: motion-reduce:
contrast-more: contrast-less:
forced-colors: not-forced-colors:
inverted-colors:
pointer-fine: pointer-coarse: pointer-none:
portrait: landscape:
noscript: print:
supports-[display:grid]: not-supports-[display:grid]:
rtl: ltr:
```

## Arbitrary Values
Use square brackets for one-off values:

```
bg-[#316ff6]
text-[22px]
w-[350px]
grid-cols-[24rem_2.5rem_minmax(0,1fr)]
max-h-[calc(100dvh-2rem)]
rounded-[11px]
shadow-[0_4px_20px_rgba(0,0,0,0.1)]
[--gutter:1rem]
```

## Arbitrary Variants
Custom selectors in square brackets:

```
[&.is-dragging]:cursor-grabbing
[&_p]:mt-4
[&>[data-active]+span]:text-blue-600
[@supports(display:grid)]:grid
```

## Common Patterns

### Card
```html
<div class="bg-white dark:bg-gray-800 rounded-xl shadow-soft border border-gray-200 dark:border-gray-700 p-6">
```

### Button (primary)
```html
<button class="inline-flex items-center gap-2 px-4 py-2 bg-brand-600 hover:bg-brand-700 focus-visible:outline-2 focus-visible:outline-offset-2 focus-visible:outline-brand-500 active:bg-brand-800 disabled:opacity-50 rounded-lg text-sm font-semibold text-white shadow-soft transition-colors duration-200">
```

### Input
```html
<input class="block w-full rounded-lg border border-gray-300 dark:border-gray-600 bg-white dark:bg-gray-800 px-3 py-2 text-sm text-gray-900 dark:text-gray-100 placeholder:text-gray-400 focus:outline-none focus:ring-2 focus:ring-brand-500 focus:border-transparent disabled:opacity-50 disabled:bg-gray-100 dark:disabled:bg-gray-900 invalid:border-red-500" />
```

### Table
```html
<div class="overflow-x-auto rounded-xl border border-gray-200 dark:border-gray-700">
    <table class="min-w-full divide-y divide-gray-200 dark:divide-gray-700">
        <thead class="bg-gray-50 dark:bg-gray-800">
            <tr>
                <th scope="col" class="px-4 py-3 text-left text-xs font-semibold text-gray-500 dark:text-gray-400 uppercase tracking-wider">...</th>
            </tr>
        </thead>
        <tbody class="divide-y divide-gray-200 dark:divide-gray-700 bg-white dark:bg-gray-900">
            <tr class="hover:bg-gray-50 dark:hover:bg-gray-800/50 transition-colors">
                <td class="px-4 py-3 text-sm text-gray-900 dark:text-gray-100">...</td>
            </tr>
        </tbody>
    </table>
</div>
```

### Modal structure
```html
<div class="fixed inset-0 z-50 flex items-center justify-center p-4">
    <div class="fixed inset-0 bg-black/50 backdrop-blur-sm" @click.outside="$el.closest('.fixed').remove()"></div>
    <div class="relative w-full max-w-lg bg-white dark:bg-gray-800 rounded-2xl shadow-xl border border-gray-200 dark:border-gray-700 scale-in">
        <div class="flex items-center justify-between px-6 py-4 border-b border-gray-200 dark:border-gray-700">
            <h2 class="text-lg font-semibold text-gray-900 dark:text-white">Title</h2>
            <button @click="$el.closest('.fixed').remove()" class="p-1 rounded-lg hover:bg-gray-100 dark:hover:bg-gray-700 transition-colors">×</button>
        </div>
        <div class="px-6 py-4">...</div>
        <div class="flex justify-end gap-3 px-6 py-4 border-t border-gray-200 dark:border-gray-700">...</div>
    </div>
</div>
```

### Empty state
```html
<div class="flex flex-col items-center justify-center py-16 text-center">
    <div class="text-5xl mb-4">📦</div>
    <h3 class="text-lg font-semibold text-gray-900 dark:text-white">No items yet</h3>
    <p class="mt-1 text-sm text-gray-500 dark:text-gray-400">Get started by creating your first item.</p>
</div>
```

### Stat card
```html
<div class="bg-white dark:bg-gray-800 rounded-xl shadow-soft border border-gray-200 dark:border-gray-700 p-6">
    <p class="text-sm font-medium text-gray-500 dark:text-gray-400">Total Sales</p>
    <p class="mt-2 text-3xl font-bold text-gray-900 dark:text-white">$12,450</p>
    <p class="mt-1 text-sm text-green-600">↑ 12.5%</p>
</div>
```

### Glass panel
```html
<div class="bg-white/80 dark:bg-gray-900/80 backdrop-blur-xl border border-white/20 dark:border-gray-700/50 rounded-2xl shadow-lg p-6">
```
