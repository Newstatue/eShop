import {
  ReactNode,
  useCallback,
  useEffect,
  useId,
  useMemo,
  useRef,
  useState,
} from "react";
import type { HTMLAttributes, SyntheticEvent } from "react";
import { AnimatePresence, motion } from "motion/react";
import Lightbox from "yet-another-react-lightbox";
import "yet-another-react-lightbox/styles.css";
import { cn } from "@/lib/utils";
import { useOutsideClick } from "@/hooks/use-outside-click";

export type ExpandableCardItem = {
  title: string;
  description: string;
  src: string;
  content: ReactNode | (() => ReactNode);
  ctaText?: string;
  ctaLink?: string;
  badge?: ReactNode;
  fallbackSrc?: string;
  gallery?: string[];
};

type ExpandableCardGridProps = {
  items?: ExpandableCardItem[];
};

type InlinePluginType = (typeof import("yet-another-react-lightbox/plugins/inline"))["default"];

export default function ExpandableCardGrid({
  items = defaultItems,
}: ExpandableCardGridProps) {
  const [active, setActive] = useState<ExpandableCardItem | boolean | null>(null);
  const [lightbox, setLightbox] = useState<{
    open: boolean;
    slides: { src: string }[];
    index: number;
  }>({
    open: false,
    slides: [],
    index: 0,
  });
  const [inlineIndex, setInlineIndex] = useState(0);
  const [inlinePlugin, setInlinePlugin] = useState<InlinePluginType | null>(null);
  const inlineInteractionRef = useRef(false);
  const id = useId();
  const ref = useRef<HTMLDivElement>(null);
  const isExpanded = typeof active === "object" && active !== null;
  const expandedCard = isExpanded ? (active as ExpandableCardItem) : null;

  useEffect(() => {
    let isMounted = true;

    import("yet-another-react-lightbox/plugins/inline")
      .then((module) => {
        if (isMounted) {
          setInlinePlugin(() => module.default);
        }
      })
      .catch(() => {
        if (isMounted) {
          setInlinePlugin(null);
        }
      });

    return () => {
      isMounted = false;
    };
  }, []);

  useEffect(() => {
    function onKeyDown(event: KeyboardEvent) {
      if (event.key === "Escape") {
        setActive(false);
      }
    }

    if (isExpanded) {
      document.body.style.overflow = "hidden";
    } else {
      document.body.style.overflow = "auto";
    }

    window.addEventListener("keydown", onKeyDown);
    return () => window.removeEventListener("keydown", onKeyDown);
  }, [isExpanded]);

  useOutsideClick(ref, () => {
    if (!lightbox.open) {
      if (inlineInteractionRef.current) {
        return;
      }
      setActive(null);
    }
  });

  const handleOpenLightbox = (gallery?: string[], startingIndex = 0) => {
    if (!gallery || gallery.length === 0) {
      return;
    }

    setInlineIndex(startingIndex);
    setLightbox({
      open: true,
      slides: gallery.map((imageSrc) => ({ src: imageSrc })),
      index: startingIndex,
    });

    inlineInteractionRef.current = false;
  };

  const handleCloseLightbox = () => {
    setLightbox((current) => ({
      ...current,
      open: false,
    }));
  };

  useEffect(() => {
    if (isExpanded) {
      setInlineIndex(0);
      return;
    }

    inlineInteractionRef.current = false;
  }, [isExpanded]);

  const getGallery = useCallback((item: ExpandableCardItem) => {
    return item.gallery && item.gallery.length > 0 ? item.gallery : [item.src];
  }, []);

  const activeGallery = useMemo(() => {
    if (!expandedCard) {
      return [];
    }

    return getGallery(expandedCard);
  }, [expandedCard, getGallery]);

  const handleInlineInteraction = useCallback((isInteracting: boolean) => {
    inlineInteractionRef.current = isInteracting;
  }, []);

  return (
    <>
      <AnimatePresence>
        {isExpanded && (
          <motion.div
            initial={{ opacity: 0 }}
            animate={{ opacity: 1 }}
            exit={{ opacity: 0 }}
            className="fixed inset-0 h-full w-full bg-background/80 backdrop-blur-sm z-10"
          />
        )}
      </AnimatePresence>
      <AnimatePresence>
        {isExpanded && expandedCard ? (
          <div className="fixed inset-0  grid place-items-center z-100">
            <motion.div
              layoutId={`card-${expandedCard.title}-${id}`}
              ref={ref}
              className="relative w-full max-w-[500px] h-full md:h-fit md:max-h-[90%] flex flex-col bg-card text-card-foreground sm:rounded-3xl overflow-hidden border border-border shadow-2xl"
            >
              <motion.button
                type="button"
                layout
                initial={{ opacity: 0 }}
                animate={{ opacity: 1 }}
                exit={{ opacity: 0 }}
                className="absolute bottom-4 right-4 z-10 flex h-10 w-10 items-center justify-center rounded-full bg-background/90 text-foreground shadow-lg ring-1 ring-border lg:hidden"
                aria-label="关闭详情"
                onClick={() => setActive(null)}
              >
                <CloseIcon />
              </motion.button>
              <div className="relative">
                <motion.div layoutId={`image-${expandedCard.title}-${id}`}>
                  <InlineGallery
                    title={expandedCard.title}
                    gallery={activeGallery}
                    fallbackSrc={expandedCard.fallbackSrc}
                    size="modal"
                    plugin={inlinePlugin}
                    index={inlineIndex}
                    onViewChange={setInlineIndex}
                    onOpenLightbox={(index) => handleOpenLightbox(activeGallery, index)}
                    onInteractionChange={handleInlineInteraction}
                  />
                </motion.div>
                {expandedCard.badge ? (
                  <motion.span
                    layout
                    className="absolute right-4 top-4 rounded-full bg-foreground/80 px-3 py-1 text-sm font-semibold text-background"
                  >
                    {expandedCard.badge}
                  </motion.span>
                ) : null}
              </div>

              <div>
                <div className="flex justify-between items-start gap-4 p-4">
                  <div className="">
                    <motion.h3
                      layoutId={`title-${expandedCard.title}-${id}`}
                      className="font-medium text-base text-card-foreground"
                    >
                      {expandedCard.title}
                    </motion.h3>
                    <motion.p
                      layoutId={`description-${expandedCard.description}-${id}`}
                      className="text-base text-muted-foreground"
                    >
                      {expandedCard.description}
                    </motion.p>
                  </div>

                  {expandedCard.ctaText && expandedCard.ctaLink ? (
                    <motion.a
                      layout
                      initial={{ opacity: 0 }}
                      animate={{ opacity: 1 }}
                      exit={{ opacity: 0 }}
                      href={expandedCard.ctaLink}
                      target="_blank"
                      className="px-4 py-3 text-sm rounded-full font-bold bg-primary text-primary-foreground shadow-md whitespace-nowrap flex-shrink-0"
                    >
                      {expandedCard.ctaText}
                    </motion.a>
                  ) : null}
                </div>
                <div className="pt-4 relative px-4">
                  <motion.div
                    layout
                    initial={{ opacity: 0 }}
                    animate={{ opacity: 1 }}
                    exit={{ opacity: 0 }}
                    className="text-muted-foreground text-xs md:text-sm lg:text-base h-100 md:h-fit pb-10 flex flex-col items-start gap-4 overflow-auto [mask:linear-gradient(to_bottom,var(--card),var(--card),transparent)] [scrollbar-width:none] [-ms-overflow-style:none] [-webkit-overflow-scrolling:touch]"
                  >
                    {typeof expandedCard.content === "function"
                      ? expandedCard.content()
                      : expandedCard.content}
                  </motion.div>
                </div>
              </div>
            </motion.div>
          </div>
        ) : null}
      </AnimatePresence>
      <ul className="mx-auto w-full max-w-6xl grid grid-cols-1 items-start gap-4 sm:grid-cols-2 lg:grid-cols-3 xl:grid-cols-4">
        {items.map((card) => {
          const gallery = getGallery(card);
          const previewSrc = gallery[0] ?? card.fallbackSrc ?? "/vite.svg";
          const isActiveCard =
            isExpanded && expandedCard && expandedCard.title === card.title;

          return (
            <motion.div
              layoutId={`card-${card.title}-${id}`}
              key={card.title}
              onClick={() => {
                setInlineIndex(0);
                setActive(card);
              }}
              className={cn(
                "p-4 flex flex-col rounded-xl cursor-pointer transition-colors bg-card text-card-foreground shadow border border-border hover:bg-muted",
                isActiveCard && "pointer-events-none invisible"
              )}
            >
              <div className="flex gap-4 flex-col  w-full">
                <motion.div
                  layoutId={`image-${card.title}-${id}`}
                  className="relative"
                >
                  <img
                    width={100}
                    height={100}
                    src={previewSrc}
                    alt={card.title}
                    className="h-60 w-full rounded-lg object-cover object-top"
                    onError={(event) => {
                      const target = event.currentTarget;
                      if (card.fallbackSrc && target.src !== card.fallbackSrc) {
                        target.onerror = null;
                        target.src = card.fallbackSrc;
                      }
                    }}
                  />
                  {card.badge ? (
                    <motion.span
                      layout
                      className="absolute right-4 top-4 rounded-full bg-foreground/80 px-3 py-1 text-sm font-semibold text-background"
                    >
                      {card.badge}
                    </motion.span>
                  ) : null}
                </motion.div>
                <div className="flex justify-center items-center flex-col">
                  <motion.h3
                    layoutId={`title-${card.title}-${id}`}
                    className="font-medium text-center md:text-left text-base text-card-foreground"
                  >
                    {card.title}
                  </motion.h3>
                  <motion.p
                    layoutId={`description-${card.description}-${id}`}
                    className="text-muted-foreground text-center md:text-left text-base"
                  >
                    {card.description}
                  </motion.p>
                </div>
              </div>
            </motion.div>
          );
        })}
      </ul>
      <Lightbox
        open={lightbox.open}
        close={handleCloseLightbox}
        slides={lightbox.slides}
        index={lightbox.index}
        on={{
          view: ({ index }) => {
            setInlineIndex(index);
            setLightbox((current) =>
              current.index === index ? current : { ...current, index }
            );
          },
        }}
      />
    </>
  );
}

type InlineGalleryProps = {
  title: string;
  gallery: string[];
  fallbackSrc?: string;
  size: "modal" | "card";
  plugin: InlinePluginType | null;
  index?: number;
  onViewChange?: (index: number) => void;
  onOpenLightbox: (index: number) => void;
  onInteractionChange?: (isInteracting: boolean) => void;
};

function InlineGallery({
  title,
  gallery,
  fallbackSrc,
  size,
  plugin,
  index,
  onViewChange,
  onOpenLightbox,
  onInteractionChange,
}: InlineGalleryProps) {
  const slides = useMemo(
    () => gallery.map((imageSrc) => ({ src: imageSrc })),
    [gallery]
  );

  const inlineStyle = useMemo(
    () => ({
      width: "100%",
      height: size === "modal" ? "320px" : "240px",
      borderRadius: size === "modal" ? "1rem" : "0.75rem",
      overflow: "hidden",
      backgroundColor: "transparent",
    }),
    [size]
  );

  const containerHeightClass = size === "modal" ? "h-80" : "h-60";
  const borderRadiusClass =
    size === "modal" ? "rounded-2xl sm:rounded-tr-lg sm:rounded-tl-lg" : "rounded-lg";

  const handleImageError = useCallback(
    (event: SyntheticEvent<HTMLImageElement>) => {
      const target = event.currentTarget;
      if (fallbackSrc && target.src !== fallbackSrc) {
        target.onerror = null;
        target.src = fallbackSrc;
      }
    },
    [fallbackSrc]
  );

  const handleInteractionStart = useCallback(
    (event: SyntheticEvent) => {
      event.stopPropagation();
      onInteractionChange?.(true);
    },
    [onInteractionChange]
  );

  const handleInteractionEnd = useCallback(
    (event: SyntheticEvent) => {
      event.stopPropagation();
      onInteractionChange?.(false);
    },
    [onInteractionChange]
  );

  const interactionHandlers = useMemo<HTMLAttributes<HTMLDivElement>>(
    () => ({
      onPointerDown: handleInteractionStart,
      onPointerUp: handleInteractionEnd,
      onPointerCancel: handleInteractionEnd,
      onPointerLeave: handleInteractionEnd,
      onMouseDown: handleInteractionStart,
      onMouseUp: handleInteractionEnd,
      onMouseLeave: handleInteractionEnd,
      onTouchStart: handleInteractionStart,
      onTouchEnd: handleInteractionEnd,
      onTouchCancel: handleInteractionEnd,
      onClick: handleInteractionEnd,
    }),
    [handleInteractionEnd, handleInteractionStart]
  );

  if (!plugin) {
    const primarySrc = slides[0]?.src ?? fallbackSrc ?? "/vite.svg";

    return (
      <div {...interactionHandlers}>
        <button
          type="button"
          className={`block w-full overflow-hidden ${containerHeightClass} ${borderRadiusClass}`}
          onClick={(event) => {
            event.stopPropagation();
            onOpenLightbox(0);
          }}
        >
          <img
            src={primarySrc}
            alt={`${title} 预览图`}
            className={`w-full ${containerHeightClass} object-cover object-top`}
            onError={handleImageError}
          />
        </button>
      </div>
    );
  }

  return (
    <div {...interactionHandlers}>
      <Lightbox
        slides={slides}
        plugins={[plugin]}
        index={index}
        inline={{ style: inlineStyle }}
        carousel={{ finite: slides.length <= 1 }}
        controller={{ closeOnBackdropClick: false }}
        styles={{
          container: { backgroundColor: "transparent" },
          slide: { backgroundColor: "transparent" },
        }}
        on={{
          view: ({ index: currentIndex }) => onViewChange?.(currentIndex),
          click: ({ index: clickedIndex }) => {
            onViewChange?.(clickedIndex);
            onOpenLightbox(clickedIndex);
          },
        }}
      />
    </div>
  );
}

const CloseIcon = () => {
  return (
    <motion.svg
      initial={{ opacity: 0 }}
      animate={{ opacity: 1 }}
      exit={{ opacity: 0 }}
      xmlns="http://www.w3.org/2000/svg"
      width="24"
      height="24"
      viewBox="0 0 24 24"
      fill="none"
      stroke="currentColor"
      strokeWidth="2"
      strokeLinecap="round"
      strokeLinejoin="round"
      className="h-4 w-4"
    >
      <path stroke="none" d="M0 0h24v24H0z" fill="none" />
      <path d="M18 6l-12 12" />
      <path d="M6 6l12 12" />
    </motion.svg>
  );
};

const defaultItems: ExpandableCardItem[] = [
  {
    description: "Lana Del Rey",
    title: "Summertime Sadness",
    src: "https://assets.aceternity.com/demos/lana-del-rey.jpeg",
    ctaText: "Visit",
    ctaLink: "https://ui.aceternity.com/templates",
    gallery: ["https://assets.aceternity.com/demos/lana-del-rey.jpeg"],
    content: () => {
      return (
        <p>
          Lana Del Rey, an iconic American singer-songwriter, is celebrated for
          her melancholic and cinematic music style. Born Elizabeth Woolridge
          Grant in New York City, she has captivated audiences worldwide with
          her haunting voice and introspective lyrics. <br /> <br /> Her songs
          often explore themes of tragic romance, glamour, and melancholia,
          drawing inspiration from both contemporary and vintage pop culture.
          With a career that has seen numerous critically acclaimed albums, Lana
          Del Rey has established herself as a unique and influential figure in
          the music industry, earning a dedicated fan base and numerous
          accolades.
        </p>
      );
    },
  }
];
